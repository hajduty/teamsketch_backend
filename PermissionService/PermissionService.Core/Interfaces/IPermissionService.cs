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
        Task<Permission> GetUserPermission(string userEmail, string roomId);
        Task<List<Permission>> GetAllPermissions(string userEmail);
        Task<bool> RemovePermissionFromUser(string userEmail, string roomId, string currentUserEmail);
        Task<Permission> AddUserPermission(Permission perm, string currentUserEmail);
        Task<Permission> UpdateUserPermission(Permission newPerm, string currentUserEmail);
        Task<List<Permission>> GetPermissionsForRoom(string roomId, string currentUserEmail);
    }
}