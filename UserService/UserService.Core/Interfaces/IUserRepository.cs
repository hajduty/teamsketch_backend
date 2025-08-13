using UserService.Core.DTOs;
using UserService.Core.Entities;

namespace UserService.Core.Interfaces;
public interface IUserRepository
{
    public Task<User> CreateUser(string email, string password);
    public Task<User> UpdateUser(int userId, string email, string password);
    public Task<User> DeleteUser(int userId);
    public Task<User?> GetUserById(int userId);
    public Task<List<User>> GetAllUsers();
    public Task<User?> GetUserByEmail(string email);
    public bool UserExists(string email);
}