using PermissionService.Core.DTOs;
using PermissionService.Core.Entities;
using PermissionService.Core.Interfaces;

namespace PermissionService.Infrastructure.Services
{
    public class PermissionService(IPermissionRepository permRepo) : IPermissionService
    {
        public async Task<Permission> AddUserPermission(Permission perm, int currentUserId)
        {
            var userPermission = await permRepo.GetUserPermissionAsync(currentUserId, perm.Room);

            if (userPermission == null || userPermission.Role != "Owner")
                throw new UnauthorizedAccessException("User is not the owner of this room.");

            var permission = await permRepo.AddUserPermissionAsync(perm);

            if (permission == null)
                throw new InvalidOperationException("Failed to add permission.");

            return permission;
        }

        public Task<List<Permission>> GetAllPermissions(int userId)
        {
            var permissions = permRepo.GetAllPermissions(userId);
            return permissions;
        }

        public Task<Permission> GetUserPermission(int userId, string roomId)
        {
            var permission = permRepo.GetUserPermissionAsync(userId, roomId);
            return permission;
        }

        public async Task<bool> RemovePermissionFromUser(int userId, string roomId, int requestUserId)
        {
            var userPermission = permRepo.GetUserPermissionAsync(requestUserId, roomId).Result;

            if (userPermission == null || userPermission.Role != "Owner")
                throw new UnauthorizedAccessException("User is not the owner of this room.");

            var result = await permRepo.RemoveUserPermissionAsync(userId, roomId);

            if (result == false)
                throw new InvalidOperationException("Failed to remove permission.");

            return result;
        }

        public Task<Permission> UpdateUserPermission(Permission newPerm, int currentUserId)
        {
            var userPermission = permRepo.GetUserPermissionAsync(currentUserId, newPerm.Room).Result;

            if (userPermission == null || userPermission.Role != "Owner")
                throw new UnauthorizedAccessException("User is not the owner of this room.");

            var permission = permRepo.UpdateUserPermissionAsync(newPerm);

            if (permission == null)
                throw new InvalidOperationException("Failed to update permission.");

            return permission;
        }
    }
}
  