using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Security.Authentication;
using UserService.Core.DTOs;
using UserService.Core.Entities;
using UserService.Core.Interfaces;

namespace UserService.Tests
{
    public class Tests
    {
        private readonly Infrastructure.Services.UserService _service;
        private readonly Mock<IUserRepository> _repo;

        public Tests()
        {
            _repo = new Mock<IUserRepository>();
            _service = new Infrastructure.Services.UserService(_repo.Object);
        }

        [Theory]
        [InlineData("newuser@example.com", "password123", false, true)]
        [InlineData("duplicate@example.com", "password123", true, false)]
        public async Task RegisterUser_Behavior(string email, string password, bool userExists, bool shouldSucceed)
        {
            var request = new CreateUserRequest { Email = email, Password = password };

            _repo.Setup(r => r.UserExists(email)).Returns(userExists);

            if (!userExists)
            {
                _repo.Setup(r => r.CreateUser(email, password))
                    .ReturnsAsync(new User { Id = Guid.NewGuid(), Email = email });
            }

            if (shouldSucceed)
            {
                var result = await _service.RegisterUser(request);
                Assert.NotNull(result);
                Assert.Equal(email, result.Email);
            }
            else
            {
                await Assert.ThrowsAsync<AuthenticationException>(() => _service.RegisterUser(request));
            }
        }

        [Theory]
        [InlineData("test@example.com", "password123", true, true)]
        [InlineData("test@example.com", "wrong-password", true, false)]
        [InlineData("notfound@example.com", "password123", false, false)]
        public async Task LoginUser_Behavior(string email, string password, bool userExists, bool shouldSucceed)
        {
            var hasher = new PasswordHasher<User>();
            User? fakeUser = null;

            if (userExists)
            {
                fakeUser = new User
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    PasswordHash = hasher.HashPassword(null!, "password123")
                };

                _repo.Setup(r => r.GetUserByEmail(email)).ReturnsAsync(fakeUser);
            }
            else
            {
                _repo.Setup(r => r.GetUserByEmail(email)).ReturnsAsync((User?)null);
            }

            if (shouldSucceed)
            {
                var result = await _service.LoginUser(email, password);
                Assert.Equal(fakeUser!.Id, result.Id);
                Assert.Equal(fakeUser.Email, result.Email);
            }
            else
            {
                await Assert.ThrowsAsync<AuthenticationException>(() => _service.LoginUser(email, password));
            }
        }
    }
}