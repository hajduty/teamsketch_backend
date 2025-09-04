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
        Task<Permission> GetUserPermission(int userId, string roomId);
        Task<List<Permission>> GetAllPermissions(int userId);
        Task<bool> RemovePermissionFromUser(int userId, string roomId, int  currentUserId);
        Task<Permission> AddUserPermission(Permission perm, int currentUserId);
        Task<Permission> UpdateUserPermission(Permission newPerm, int currentUserId);
        Task<List<Permission>> GetPermissionsForRoom(string roomId, int currentUserId);
    }
}