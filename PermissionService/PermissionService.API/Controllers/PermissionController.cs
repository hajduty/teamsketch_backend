using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PermissionService.Core.DTOs;
using PermissionService.Core.Entities;
using PermissionService.Core.Interfaces;
using PermissionService.Infrastructure.Migrations;
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
        private string GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(claim))
                throw new UnauthorizedAccessException("Invalid user claim.");

            return claim;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserPermissionAsync([FromQuery] string userId, [FromQuery] string roomId)
        {
            if (GetCurrentUserId() != userId)
            {
                return Forbid();
            }

            var result = await permService.GetUserPermission(userId, roomId);

            if (result == null)
            {
                return NotFound($"No permission found for user {userId} in room {roomId}.");
            }

            return Ok(result);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllUserPermissions([FromQuery] string userId)
        {
            if (GetCurrentUserId() != userId)
            {
                return Forbid();
            }

            var result = await permService.GetAllPermissions(userId);

            if (result == null)
            {
                return NotFound($"No permissions found for user {userId}.");
            }

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddUserPermissionAsync([FromBody] Core.Entities.Permission permission)
        {
            var currentUserId = GetCurrentUserId();
            var result = await permService.AddUserPermission(permission, currentUserId);

            if (result == null)
            {
                return BadRequest("Failed to add permission.");
            }

            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> EditUserPermission([FromBody] Core.Entities.Permission perm)
        {
            var currentUserId = GetCurrentUserId();

            var result = await permService.UpdateUserPermission(perm, currentUserId);

            if (result == null)
            {
                return BadRequest("Failed to update permission.");
            }

            return Ok(result);
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveUserPermissionAsync([FromQuery] string userEmail, string roomId)
        {
            var currentUserId = GetCurrentUserId();

            var result = await permService.RemovePermissionFromUser(userEmail, roomId, currentUserId);

            if (result == false)
            {
                return BadRequest("Failed to remove permission.");
            }

            return Ok(result);
        }

        [HttpGet("room")]
        public async Task<IActionResult> GetRoomPermissions([FromQuery] string roomId)
        {
            var currentUserId = GetCurrentUserId();

            var permissions = await permService.GetPermissionsForRoom(roomId, currentUserId);

            if (permissions == null || permissions.Count == 0)
            {
                return NotFound($"No permissions found for user {currentUserId}.");
            }

            return Ok(permissions);
        }
    }
}
