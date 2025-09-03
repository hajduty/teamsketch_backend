using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PermissionService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionService : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Permission Service is running.");
        }

        [HttpPost]
        public IActionResult Post()
        {
            return Ok("Post endpoint hit.");
        }

        [HttpPut]
        public IActionResult Put()
        {
            return Ok("Put endpoint hit.");
        }

        [HttpDelete]
        public IActionResult Delete()
        {
            return Ok("Delete endpoint hit.");
        }
    }
}
