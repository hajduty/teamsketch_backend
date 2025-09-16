using PermissionService.Core.Entities;

namespace PermissionService.Core.Interfaces
{
    public interface IPermissionRepository
    {
        Task<List<Permission>> GetAllPermissions(string userEmail);
        Task<Permission> GetUserPermissionAsync(string currentUserEmail, string roomId, bool asNoTracking = false);
        Task<Permission> AddUserPermissionAsync(Permission perm);
        Task<bool> RemoveUserPermissionAsync(string userEmail, string roomId);
        Task<Permission> UpdateUserPermissionAsync(Permission newPerm);
        Task<Permission?> GetOwnerPermissionAsync(string room);
        Task<List<Permission>> GetPermissionsForRoomAsync(string roomId);
    }
}
