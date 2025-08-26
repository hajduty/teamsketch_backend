using UserService.Core.DTOs;

namespace UserService.Core.Interfaces;

public interface IUserService
{
    public Task<UserResponse> RegisterUser(CreateUserRequest request);
    public Task<UserResponse> LoginUser(string email, string password);
}