using Microsoft.EntityFrameworkCore;
using PermissionService.Core.Entities;
using PermissionService.Core.Interfaces;
using PermissionService.Infrastructure.Data;

namespace PermissionService.Infrastructure.Repositories
{
    public class PermissionRepository(AppDbContext context) : IPermissionRepository
    {
        public async Task<Permission> AddUserPermissionAsync(Permission perm)
        {
            var permission = new Permission { Role = perm.Role, Room = perm.Room, UserId = perm.UserId };

            await context.Permissions.AddAsync(permission);

            await context.SaveChangesAsync();

            return permission;
        }

        public Task<List<Permission>> GetAllPermissions(int userId)
        {
            var permissions = context.Permissions.Where(p => p.UserId == userId).ToListAsync();

            return permissions;
        }

        public Task<Permission> GetUserPermissionAsync(int userId, string roomId)
        {
            var permission = context.Permissions.FirstOrDefaultAsync(p => p.UserId == userId && p.Room == roomId);
            return permission;
        }

        public async Task<bool> RemoveUserPermissionAsync(int userId, string roomId)
        {
            var permission = context.Permissions.FirstOrDefault(p => p.UserId == userId && p.Room == roomId);

            if (permission == null)
                return false;

            context.Permissions.Remove(permission);
            await context.SaveChangesAsync();

            return true;
        }

        public async Task<Permission> UpdateUserPermissionAsync(Permission newPerm)
        {
            var permission = context.Permissions.FirstOrDefault(p => p.UserId == newPerm.UserId && p.Room == newPerm.Room);

            if (permission == null)
                return new Permission();

            permission.Role = newPerm.Role;

            await context.SaveChangesAsync();
            return permission;
        }
    }
}
