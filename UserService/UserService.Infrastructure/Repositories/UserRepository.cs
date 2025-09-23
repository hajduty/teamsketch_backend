using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using UserService.Core.DTOs;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
using UserService.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;

namespace UserService.Infrastructure.Repositories;

public class UserRepository(AppDbContext context) : IUserRepository
{
    public async Task<User> CreateUser(string email, string password)
    {
        var newUser = new User
        {
            Email = email,
            PasswordHash = password
        };
        var hasher = new PasswordHasher<User>();
        newUser.PasswordHash = hasher.HashPassword(newUser, newUser.PasswordHash);

        await context.Users.AddAsync(newUser);

        await context.SaveChangesAsync();

        return newUser;
    }

    public async Task<User?> UpdateUser(Guid userId, string email, string newPassword)
    {
        var user = await context.Users.FindAsync(userId);

        if (user == null)
            return null;

        if (!string.IsNullOrWhiteSpace(email))
            user.Email = email;

        if (!string.IsNullOrWhiteSpace(newPassword))
        {
            var hasher = new PasswordHasher<User>();
            user.PasswordHash = hasher.HashPassword(user, newPassword);
        }

        await context.SaveChangesAsync();
        return user;
    }

    public Task<User> DeleteUser(Guid userId)
    {
        throw new NotImplementedException();
    }

    public async Task<User?> GetUserById(Guid userId) => await context.Users.FindAsync(userId);

    public async Task<List<User>> GetAllUsers()
    {
        return await context.Users.ToListAsync();
    }

    public async Task<User?> GetUserByEmail(string email)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public bool UserExists(string email)
    {
        return context.Users.Any(u => u.Email == email);
    }
}
