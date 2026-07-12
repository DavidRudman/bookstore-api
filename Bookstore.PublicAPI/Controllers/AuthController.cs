using Bookstore.PublicAPI.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Bookstore.PublicAPI.Controllers
{
    [EnableRateLimiting("fixed")]
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly JwtTokenService _tokens;
        public AuthController(JwtTokenService tokens) => _tokens = tokens;

        // Demo users only. Real apps: user store + hashed passwords (ASP.NET Identity / PBKDF2).
        private static readonly Dictionary<string, (string Password, string Role)> Users = new()
        {
            ["reader"] = ("Reader123!", Roles.Read),
            ["writer"] = ("Writer123!", Roles.ReadWrite)
        };

        [HttpPost("login")]
        [AllowAnonymous]
        public ActionResult<object> Login(LoginRequest request)
        {
            if (!Users.TryGetValue(request.Username, out var u) || u.Password != request.Password)
                return Unauthorized();

            return Ok(new { token = _tokens.CreateToken(request.Username, u.Role) });
        }
    }

    public record LoginRequest(string Username, string Password);
}
