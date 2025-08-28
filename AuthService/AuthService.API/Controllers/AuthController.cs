using AuthService.Core.DTOs;
using AuthService.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthRequest request)
        {
            var result = await authService.LoginAsync(request);

            if (result.Success)
                return Ok(result);
            else
                return Unauthorized(result);
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AuthRequest request)
        {
            var result = await authService.RegisterAsync(request);

            if (result.Success)
                return Ok(result);
            else
                return Unauthorized(result);
        }
    }
}
