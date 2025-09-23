using UserService.Core.DTOs;
using UserService.Core.Entities;

namespace UserService.Core.Interfaces;
public interface IUserRepository
{
    public Task<User> CreateUser(string email, string password);
    public Task<User?> UpdateUser(Guid userId, string email, string newPassword);
    public Task<User> DeleteUser(Guid userId);
    public Task<User?> GetUserById(Guid userId);
    public Task<List<User>> GetAllUsers();
    public Task<User?> GetUserByEmail(string email);
    public bool UserExists(string email);
}