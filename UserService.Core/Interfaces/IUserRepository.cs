using UserService.Core.DTOs;
using UserService.Core.Entities;

namespace UserService.Core.Interfaces;
public interface IUserRepository
{
    public Task CreateUser(string email, string password);
    public Task UpdateUser(int userId, string email, string password);
    public Task DeleteUser(int userId);
    public Task<User?> GetUserById(int userId);
    public Task<List<User>> GetAllUsers();
    public Task<User?> GetUserByEmail(string email);
    public bool UserExists(string email);
}