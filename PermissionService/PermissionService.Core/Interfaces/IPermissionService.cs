using PermissionService.Core.DTOs;
using PermissionService.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermissionService.Core.Interfaces
{
    public interface IPermissionService
    {
        Task<Permission> GetUserPermission(string userId, string roomId);
        Task<List<Permission>> GetAllPermissions(string userId);
        Task<bool> RemovePermissionFromUser(string userId, string roomId, string currentUserId);
        Task<Permission> AddUserPermission(Permission perm, string currentUserId);
        Task<Permission> UpdateUserPermission(Permission newPerm, string currentUserId);
        Task<List<Permission>> GetPermissionsForRoom(string roomId, string currentUserId);
    }
}