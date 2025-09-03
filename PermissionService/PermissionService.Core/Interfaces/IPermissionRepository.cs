using PermissionService.Core.Entities;

namespace PermissionService.Core.Interfaces
{
    public interface IPermissionRepository
    {
        Task<List<Permission>> GetAllPermissions(int userId);
        Task<Permission> GetUserPermissionAsync(int userId, string roomId);
        Task<Permission> AddUserPermissionAsync(Permission perm);
        Task<bool> RemoveUserPermissionAsync(int userId, string roomId);
        Task<Permission> UpdateUserPermissionAsync(Permission newPerm);
    }
}
