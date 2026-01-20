namespace Tredo.Api.Contracts
{
    public record RegisterRequest(string Email, string Password);
    public record LoginRequest(string Email, string Password);
    public record RefreshRequest(string RefreshToken);

    public record AuthResponse(
        Guid UserId,
        string Email,
        string AccessToken,
        DateTime AccessTokenExpiresUtc,
        string RefreshToken,
        DateTime RefreshTokenExpiresUtc
    );
}
