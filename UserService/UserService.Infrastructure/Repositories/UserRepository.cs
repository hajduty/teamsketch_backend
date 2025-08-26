using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using UserService.Core.DTOs;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
using UserService.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;

namespace UserService.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User> CreateUser(string email, string password)
    {
        var newUser = new User
        {
            Email = email,
            PasswordHash = password
        };
        var hasher = new PasswordHasher<User>();
        newUser.PasswordHash = hasher.HashPassword(newUser, newUser.PasswordHash);

        await _context.Users.AddAsync(newUser);

        await _context.SaveChangesAsync();

        return newUser;
    }

    public async Task<User?> UpdateUser(int userId, string email, string newPassword)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return null;

        if (!string.IsNullOrWhiteSpace(email))
            user.Email = email;

        if (!string.IsNullOrWhiteSpace(newPassword))
        {
            var hasher = new PasswordHasher<User>();
            user.PasswordHash = hasher.HashPassword(user, newPassword);
        }

        await _context.SaveChangesAsync();
        return user;
    }

    public Task<User> DeleteUser(int userId)
    {
        throw new NotImplementedException();
    }

    public async Task<User?> GetUserById(int userId) => await _context.Users.FindAsync(userId);

    public async Task<List<User>> GetAllUsers()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<User?> GetUserByEmail(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public bool UserExists(string email)
    {
        return _context.Users.Any(u => u.Email == email);
    }
}
