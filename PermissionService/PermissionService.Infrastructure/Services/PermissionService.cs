using PermissionService.Core.DTOs;
using PermissionService.Core.Entities;
using PermissionService.Core.Interfaces;

namespace PermissionService.Infrastructure.Services
{
    public class PermissionService(IPermissionRepository permRepo) : IPermissionService
    {
        public async Task<Permission> AddUserPermission(Permission perm, int currentUserId)
        {
            var currentUserPermission = await permRepo.GetUserPermissionAsync(currentUserId, perm.Room, true);

            if (currentUserPermission == null)
            {
                var existingOwner = await permRepo.GetOwnerPermissionAsync(perm.Room);
                if (existingOwner != null)
                    throw new UnauthorizedAccessException("User is not the owner of this room.");

                var ownerPermission = new Permission
                {
                    UserId = currentUserId,
                    Room = perm.Room,
                    Role = "Owner"
                };

                var createdOwner = await permRepo.AddUserPermissionAsync(ownerPermission);

                if (createdOwner == null)
                    throw new InvalidOperationException("Failed to create room with owner.");

                return createdOwner;
            }

            if (currentUserPermission.Role != "Owner")
                throw new UnauthorizedAccessException("User is not the owner of this room.");

            var targetUserPermission = await permRepo.GetUserPermissionAsync(perm.UserId, perm.Room);
            if (targetUserPermission != null)
                throw new InvalidOperationException("User already has a role in this room.");

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

        public async Task<List<Permission>> GetPermissionsForRoom(string roomId, int currentUserId)
        {
            var currentUserPermission = await permRepo.GetUserPermissionAsync(currentUserId, roomId, true);

            if (currentUserPermission == null || currentUserPermission.Role != "Owner")
                throw new UnauthorizedAccessException("User is not the owner of this room.");

            var permissions = await permRepo.GetPermissionsForRoomAsync(roomId);

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
