using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tredo.Api.Contracts;
using Tredo.Api.Data;
using Tredo.Api.Services;

namespace Tredo.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _auth;

        public AuthController(AuthService auth, AppDbContext db)
        {
            _auth = auth;

        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register(RegisterRequest req)
        {
            try { return Ok(await _auth.RegisterAsync(req)); }
            catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest req)
        {
            try { return Ok(await _auth.LoginAsync(req)); }
            catch (InvalidOperationException ex) { return Unauthorized(new { error = ex.Message }); }
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponse>> Refresh(RefreshRequest req)
        {
            try { return Ok(await _auth.RefreshAsync(req)); }
            catch (InvalidOperationException ex) { return Unauthorized(new { error = ex.Message }); }
        }




    }
}
