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
            var permission = new Permission { Role = perm.Role, Room = perm.Room, UserId = perm.UserId, UserEmail = perm.UserEmail };

            await context.Permissions.AddAsync(permission);

            await context.SaveChangesAsync();

            return permission;
        }

        public Task<List<Permission>> GetAllPermissions(string userId)
        {
            var permissions = context.Permissions.Where(p => p.UserId == userId).ToListAsync();

            return permissions;
        }

        public async Task<Permission> GetUserPermissionAsync(string currentUserId, string roomId, bool asNoTracking = false)
        {
            var query = context.Permissions.Where(p => p.UserId == currentUserId && p.Room == roomId);
            if (asNoTracking) query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync();
        }

        public async Task<bool> RemoveUserPermissionAsync(string userId, string roomId)
        {
            var permission = await context.Permissions.Where(p => p.UserId == userId && p.Room == roomId).FirstOrDefaultAsync();

            if (permission == null)
                return false;

            context.Permissions.Remove(permission);
            await context.SaveChangesAsync();

            return true;
        }

        public async Task<Permission?> GetOwnerPermissionAsync(string room)
        {
            return await context.Permissions
                .Where(p => p.Room == room && p.Role == "Owner")
                .FirstOrDefaultAsync();
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

        public Task<List<Permission>> GetPermissionsForRoomAsync(string roomId)
        {
            var permissions = context.Permissions.Where(p => p.Room == roomId).ToListAsync();

            return permissions;
        }
    }
}
