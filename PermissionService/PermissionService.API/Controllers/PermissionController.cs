using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PermissionService.Core.Entities;
using PermissionService.Core.Interfaces;
using System.Security;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PermissionService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PermissionController(IPermissionService permService) : ControllerBase
    {
        private string GetCurrentUserEmail()
        {
            var claim = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(claim))
                throw new UnauthorizedAccessException("Invalid user claim.");

            return claim;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserPermissionAsync([FromQuery] string userEmail, [FromQuery] string roomId)
        {
            if (GetCurrentUserEmail() != userEmail)
            {
                return Forbid();
            }

            var result = await permService.GetUserPermission(userEmail, roomId);

            if (result == null)
            {
                return NotFound($"No permission found for user {userEmail} in room {roomId}.");
            }

            return Ok(result);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllUserPermissions([FromQuery] string userEmail)
        {
            if (GetCurrentUserEmail() != userEmail)
            {
                return Forbid();
            }

            var result = await permService.GetAllPermissions(userEmail);

            if (result == null)
            {
                return NotFound($"No permissions found for user {userEmail}.");
            }

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddUserPermissionAsync([FromBody] Core.Entities.Permission permission)
        {
            var currentUserEmail = GetCurrentUserEmail();
            var result = await permService.AddUserPermission(permission, currentUserEmail);

            if (result == null)
            {
                return BadRequest("Failed to add permission.");
            }

            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> EditUserPermission([FromBody] Core.Entities.Permission perm)
        {
            var currentUserEmail = GetCurrentUserEmail();

            var result = await permService.UpdateUserPermission(perm, currentUserEmail);

            if (result == null)
            {
                return BadRequest("Failed to update permission.");
            }

            return Ok(result);
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveUserPermissionAsync([FromBody] string userEmail, string roomId)
        {
            var currentUserEmail = GetCurrentUserEmail();

            var result = await permService.RemovePermissionFromUser(userEmail, roomId, currentUserEmail);

            if (result == false)
            {
                return BadRequest("Failed to remove permission.");
            }

            return Ok(result);
        }

        [HttpGet("room")]
        public async Task<IActionResult> GetRoomPermissions([FromQuery] string roomId)
        {
            var currentUserEmail = GetCurrentUserEmail();

            var permissions = await permService.GetPermissionsForRoom(roomId, currentUserEmail);

            if (permissions == null || permissions.Count == 0)
            {
                return NotFound($"No permissions found for user {currentUserEmail}.");
            }

            return Ok(permissions);
        }
    }
}
