using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Authentication;
using UserService.Core.DTOs;
using UserService.Core.Entities;
using UserService.Core.Interfaces;

namespace UserService.Infrastructure.Services;

public class UserService(IUserRepository userRepository) : IUserService
{
    public async Task<UserResponse> RegisterUser(CreateUserRequest request)
    {
        var existingUser = await userRepository.GetUserByEmail(request.Email);
        if (existingUser != null)
        {
            throw new AuthenticationException("Email already registered.");
        }

        var user = await userRepository.CreateUser(request.Email, request.Password);

        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
        };
    }

    public async Task<UserResponse> LoginUser(string email, string password)
    {
        var user = await userRepository.GetUserByEmail(email);
        if (user == null)
        {
            throw new AuthenticationException("Invalid email or password.");
        }

        var hasher = new PasswordHasher<User>();

        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, password);

        if (result != PasswordVerificationResult.Success)
        {
            throw new AuthenticationException("Invalid email or password.");
        }

        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
        };
    }
}