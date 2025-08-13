using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace UserService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        [HttpPost("login")]
        public IActionResult Login([FromBody] UserService.Core.DTOs.CreateUserRequest request)
        {
            // Simulate user login logic
            if (request.Email == "")
            {
                return BadRequest("Email is required.");
            }
        }

        [HttpPost("register")]
        public IActionResult Login([FromBody] UserService.Core.DTOs.CreateUserRequest request)
        {
            // Simulate user login logic
            if (request.Email == "")
            {
                return BadRequest("Email is required.");
            }
        }
    }
}
