using UserService.Core.DTOs;

namespace UserService.Core.Interfaces;

public interface IUserService
{
    public UserResponse RegisterUser(CreateUserRequest request);
    public UserResponse LoginUser(string email, string password);
}