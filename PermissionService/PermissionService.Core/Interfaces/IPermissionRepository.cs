using PermissionService.Core.Entities;

namespace PermissionService.Core.Interfaces
{
    public interface IPermissionRepository
    {
        Task<List<Permission>> GetAllPermissions(string userId);
        Task<Permission> GetUserPermissionAsync(string currentUserId, string roomId, bool asNoTracking = false);
        Task<Permission> AddUserPermissionAsync(Permission perm);
        Task<bool> RemoveUserPermissionAsync(string userId, string roomId);
        Task<Permission> UpdateUserPermissionAsync(Permission newPerm);
        Task<Permission?> GetOwnerPermissionAsync(string room);
        Task<List<Permission>> GetPermissionsForRoomAsync(string roomId);
    }
}
