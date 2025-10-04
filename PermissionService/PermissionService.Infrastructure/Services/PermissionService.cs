using Grpc.Core;
using PermissionService.Core.Entities;
using PermissionService.Core.Interfaces;
using UserService.Grpc;

namespace PermissionService.Infrastructure.Services
{
    public class PermissionService(IPermissionRepository permRepo, IPermissionNotifier notifier, User.UserClient userClient, IPermissionPublisher redisPublisher) : IPermissionService
    {
        public async Task<Permission> AddUserPermission(Permission perm, string currentUserId)
        {
            var currentUserPermission = await permRepo.GetUserPermissionAsync(currentUserId, perm.Room, true);

            if (currentUserPermission == null)
            {
                var existingOwner = await permRepo.GetOwnerPermissionAsync(perm.Room);

                if (existingOwner != null)
                    throw new UnauthorizedAccessException("User is not the owner of this room.");

                var ownerPermission = new Permission
                {
                    UserId = perm.UserId,
                    Room = perm.Room,
                    Role = "Owner",
                    UserEmail = perm.UserEmail
                };

                var createdOwner = await permRepo.AddUserPermissionAsync(ownerPermission);

                if (createdOwner == null)
                    throw new InvalidOperationException("Failed to create room with owner.");

                _ = notifier.NotifyPermissionAdded(currentUserId, perm.Room, "Owner");

                return createdOwner;
            }

            if (currentUserPermission.Role != "Owner")
                throw new UnauthorizedAccessException("User is not the owner of this room.");

            var targetUserPermission = await permRepo.GetUserPermissionAsync(perm.UserId, perm.Room);
            if (targetUserPermission != null)
                throw new InvalidOperationException("User already has a role in this room.");

            UserResponse trueUser;
            try
            {
                trueUser = await userClient.EmailToUidAsync(new EmailToUidRequest { Email = perm.UserEmail });
            }
            catch (RpcException ex) when (ex.Status.StatusCode == StatusCode.NotFound)
            {
                throw new InvalidOperationException("User does not exist.");
            }

            perm.UserId = trueUser.Id;

            var permission = await permRepo.AddUserPermissionAsync(perm);

            if (permission == null)
                throw new InvalidOperationException("Failed to add permission.");

            _ = notifier.NotifyPermissionAdded(perm.UserId, perm.Room, perm.Role);

            return permission;
        }

        public Task<List<Permission>> GetAllPermissions(string userId)
        {
            var permissions = permRepo.GetAllPermissions(userId);
            return permissions;
        }

        public async Task<List<Permission>> GetPermissionsForRoom(string roomId, string currentUserId)
        {
            var currentUserPermission = await permRepo.GetUserPermissionAsync(currentUserId, roomId, true);

            if (currentUserPermission == null)
                throw new UnauthorizedAccessException("User does not have access to this room.");

            var permissions = await permRepo.GetPermissionsForRoomAsync(roomId);

            return permissions;
        }

        public Task<Permission> GetUserPermission(string userId, string roomId)
        {
            var permission = permRepo.GetUserPermissionAsync(userId, roomId);
            return permission;
        }

        public async Task<bool> RemovePermissionFromUser(string userId, string roomId, string requestUserId)
        {
            var userPermission = await permRepo.GetUserPermissionAsync(requestUserId, roomId);

            if (userPermission == null || userPermission.Role != "Owner")
                throw new UnauthorizedAccessException("User is not the owner of this room.");

            var result = await permRepo.RemoveUserPermissionAsync(userId, roomId);

            if (result == false)
                throw new InvalidOperationException("Failed to remove permission.");

            _ = notifier.NotifyPermissionChanged(userId, roomId, "None");

            await redisPublisher.PublishKickRequestAsync(userId,roomId, "Permission changed");

            return result;
        }

        public async Task<Permission> UpdateUserPermission(Permission newPerm, string currentUserId)
        {
            var userPermission = await permRepo.GetUserPermissionAsync(currentUserId, newPerm.Room);

            if (userPermission == null || userPermission.Role != "Owner")
                throw new UnauthorizedAccessException("User is not the owner of this room.");

            if (newPerm.UserId == userPermission.UserId && userPermission.Role == "Owner")
                throw new InvalidOperationException("Owner permission cannot be changed.");

            var permission = await permRepo.UpdateUserPermissionAsync(newPerm);

            if (permission == null)
                throw new InvalidOperationException("Failed to update permission.");

            _ = notifier.NotifyPermissionChanged(newPerm.UserId, newPerm.Room, newPerm.Role);

            return permission;
        }
    }
}
