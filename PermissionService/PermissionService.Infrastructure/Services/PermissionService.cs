using PermissionService.Core.Entities;
using PermissionService.Core.Interfaces;

namespace PermissionService.Infrastructure.Services
{
    public class PermissionService(IPermissionRepository permRepo, IPermissionNotifier notifier) : IPermissionService
    {
        public async Task<Permission> AddUserPermission(Permission perm, string currentUserEmail)
        {
            var currentUserPermission = await permRepo.GetUserPermissionAsync(currentUserEmail, perm.Room, true);

            if (currentUserPermission == null)
            {
                var existingOwner = await permRepo.GetOwnerPermissionAsync(perm.Room);
                if (existingOwner != null)
                    throw new UnauthorizedAccessException("User is not the owner of this room.");

                var ownerPermission = new Permission
                {
                    UserEmail = perm.UserEmail,
                    Room = perm.Room,
                    Role = "Owner"
                };

                var createdOwner = await permRepo.AddUserPermissionAsync(ownerPermission);

                if (createdOwner == null)
                    throw new InvalidOperationException("Failed to create room with owner.");

                //_ = notifier.NotifyPermissionAdded(currentUserEmail, perm.Room, "Owner");

                return createdOwner;
            }

            if (currentUserPermission.Role != "Owner")
                throw new UnauthorizedAccessException("User is not the owner of this room.");

            var targetUserPermission = await permRepo.GetUserPermissionAsync(perm.UserEmail, perm.Room);
            if (targetUserPermission != null)
                throw new InvalidOperationException("User already has a role in this room.");

            var permission = await permRepo.AddUserPermissionAsync(perm);

            if (permission == null)
                throw new InvalidOperationException("Failed to add permission.");

            //_ = notifier.NotifyPermissionAdded(perm.UserId, perm.Room, perm.Role);

            return permission;
        }

        public Task<List<Permission>> GetAllPermissions(string userEmail)
        {
            var permissions = permRepo.GetAllPermissions(userEmail);
            return permissions;
        }

        public async Task<List<Permission>> GetPermissionsForRoom(string roomId, string currentUserEmail)
        {
            var currentUserPermission = await permRepo.GetUserPermissionAsync(currentUserEmail, roomId, true);

            if (currentUserPermission == null || currentUserPermission.Role != "Owner")
                throw new UnauthorizedAccessException("User is not the owner of this room.");

            var permissions = await permRepo.GetPermissionsForRoomAsync(roomId);

            return permissions;
        }

        public Task<Permission> GetUserPermission(string userEmail, string roomId)
        {
            var permission = permRepo.GetUserPermissionAsync(userEmail, roomId);
            return permission;
        }

        public async Task<bool> RemovePermissionFromUser(string userEmail, string roomId, string requestUserEmail)
        {
            var userPermission = permRepo.GetUserPermissionAsync(requestUserEmail, roomId).Result;

            if (userPermission == null || userPermission.Role != "Owner")
                throw new UnauthorizedAccessException("User is not the owner of this room.");

            var result = await permRepo.RemoveUserPermissionAsync(userEmail, roomId);

            if (result == false)
                throw new InvalidOperationException("Failed to remove permission.");

            return result;
        }

        public async Task<Permission> UpdateUserPermission(Permission newPerm, string currentUserEmail)
        {
            var userPermission = await permRepo.GetUserPermissionAsync(currentUserEmail, newPerm.Room);

            if (userPermission == null || userPermission.Role != "Owner")
                throw new UnauthorizedAccessException("User is not the owner of this room.");

            var permission = await permRepo.UpdateUserPermissionAsync(newPerm);

            if (permission == null)
                throw new InvalidOperationException("Failed to update permission.");

            //_ = notifier.NotifyPermissionChanged(newPerm.UserEmail, newPerm.Room, newPerm.Role);

            return permission;
        }
    }
}
