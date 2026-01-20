using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Tredo.Api.Contracts;
using Tredo.Api.Data;
using Tredo.Api.Models;

namespace Tredo.Api.Services
{
    public class AuthService
    {
        private readonly AppDbContext _db;
        private readonly JwtOptions _jwt;

        public AuthService(AppDbContext db, IOptions<JwtOptions> jwt)
        {
            _db = db;
            _jwt = jwt.Value;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest req)
        {
            var email = req.Email.Trim().ToLowerInvariant();

            if (await _db.Users.AnyAsync(x => x.Email == email))
                throw new InvalidOperationException("Email already exists");

            var user = new User
            {
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password)
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return await IssueTokensAsync(user);
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest req)
        {
            var email = req.Email.Trim().ToLowerInvariant();
            var user = await _db.Users.Include(x => x.RefreshTokens)
                .FirstOrDefaultAsync(x => x.Email == email);

            if (user == null) throw new InvalidOperationException("Invalid credentials");
            if (!BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
                throw new InvalidOperationException("Invalid credentials");

            return await IssueTokensAsync(user);
        }

        public async Task<AuthResponse> RefreshAsync(RefreshRequest req)
        {
            var hash = Sha256(req.RefreshToken);

            var rt = await _db.RefreshTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.TokenHash == hash);

            if (rt == null || !rt.IsActive)
                throw new InvalidOperationException("Invalid refresh token");

            // rotate refresh token
            rt.RevokedUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return await IssueTokensAsync(rt.User);
        }

        private async Task<AuthResponse> IssueTokensAsync(User user)
        {
            var now = DateTime.UtcNow;

            var accessExp = now.AddMinutes(_jwt.AccessTokenMinutes);
            var accessToken = CreateJwt(user, accessExp);

            var refreshToken = CreateSecureToken();
            var refreshExp = now.AddDays(_jwt.RefreshTokenDays);

            var rt = new RefreshToken
            {
                UserId = user.Id,
                TokenHash = Sha256(refreshToken),
                ExpiresUtc = refreshExp
            };

            _db.RefreshTokens.Add(rt);

            // optional: clean old tokens
            var old = await _db.RefreshTokens
                .Where(x =>
                                 x.UserId == user.Id &&
                                 x.RevokedUtc == null &&
                                 x.ExpiresUtc > DateTime.UtcNow
)
                .OrderByDescending(x => x.CreatedUtc)
                .Skip(10)
                .ToListAsync();
            if (old.Count > 0) _db.RefreshTokens.RemoveRange(old);

            await _db.SaveChangesAsync();

            return new AuthResponse(
                user.Id,
                user.Email,
                accessToken,
                accessExp,
                refreshToken,
                refreshExp
            );
        }

        private string CreateJwt(User user, DateTime expiresUtc)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

            var token = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: expiresUtc,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string CreateSecureToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes);
        }

        private static string Sha256(string input)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(bytes);
        }

    }
}
