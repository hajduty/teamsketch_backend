using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.Infrastructure.Data;
using UserService.Infrastructure.Repositories;
using Xunit;

namespace UserService.Tests
{
    public class IntegrationTests
    {
        private readonly AppDbContext _context;
        private readonly UserRepository _repo;

        public IntegrationTests() 
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _repo = new UserRepository(_context);
        }  

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task CreateUser_ShouldAddUserWithHashedPassword()
        {
            var user = await _repo.CreateUser("test@example.com", "password123");

            Assert.NotNull(user);
            Assert.Equal("test@example.com", user.Email);
            Assert.NotNull(user.PasswordHash);
            Assert.NotEqual("password123", user.PasswordHash);

            var inDb = await _context.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
            Assert.NotNull(inDb);
        }

        [Theory]
        [InlineData("existing@example.com", true)]
        [InlineData("notfound@example.com", false)]
        public async Task GetUserByEmail_ReturnsExpectedResult(string email, bool shouldExist)
        {
            if (shouldExist)
            {
                await _repo.CreateUser(email, "password123");
            }

            var user = await _repo.GetUserByEmail(email);

            if (shouldExist)
            {
                Assert.NotNull(user);
                Assert.Equal(email, user.Email);
            }
            else
            {
                Assert.Null(user);
            }
        }
    }
}
