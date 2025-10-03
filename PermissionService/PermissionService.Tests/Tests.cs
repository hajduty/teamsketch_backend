using Grpc.Core;
using Moq;
using PermissionService.Core.Entities;
using PermissionService.Core.Interfaces;
using PermissionService.Infrastructure;
using PermissionService.Infrastructure.Migrations;
using UserService.Grpc;
using static UserService.Grpc.User;

namespace PermissionService.Tests
{
    public class Tests
    {
        private readonly Mock<IPermissionRepository> _permRepo;
        private readonly Mock<IPermissionNotifier> _permNotifier;
        private readonly Mock<User.UserClient> _userClient;
        private readonly Infrastructure.Services.PermissionService _permService;
        private readonly Mock<IPermissionPublisher> _permPublisher;

        private static AsyncUnaryCall<T> CreateAsyncUnaryCall<T>(T response)
        {
            return new AsyncUnaryCall<T>(
                Task.FromResult(response),
                Task.FromResult(new Grpc.Core.Metadata()),
                () => Grpc.Core.Status.DefaultSuccess,
                () => new Grpc.Core.Metadata(),
                () => { }
            );
        }

        public Tests()
        {
            _permRepo = new Mock<IPermissionRepository>();
            _permNotifier = new Mock<IPermissionNotifier>();
            _userClient = new Mock<User.UserClient>();
            _permPublisher = new Mock<IPermissionPublisher>();
            _permService = new Infrastructure.Services.PermissionService(_permRepo.Object, _permNotifier.Object, _userClient.Object, _permPublisher.Object);
        }

        [Fact]
        public async Task AddUserPermission_ShouldSucceed_WhenCurrentUserIsOwner()
        {
            var targetPermission = new Permission { Role = "Editor", UserId = "AddedUserId", UserEmail = "AddedUserEmail", Room = "roomId" };
            var currentUserPermission = new Permission { Role = "Owner", UserId = "OwnerUserId", UserEmail = "OwnerUserEmail", Room = "roomId" };

            _permRepo.Setup(r => r.GetUserPermissionAsync(currentUserPermission.UserId, "roomId", true))
                .ReturnsAsync(currentUserPermission);

            _permRepo.Setup(r => r.GetUserPermissionAsync(targetPermission.UserId, "roomId", false))
                .ReturnsAsync((Permission?)null);

            _permRepo.Setup(r => r.AddUserPermissionAsync(targetPermission))
                .ReturnsAsync(targetPermission);

            _userClient.Setup(c => c.EmailToUidAsync(It.IsAny<EmailToUidRequest>(), null, null, default))
                .Returns(CreateAsyncUnaryCall(new UserResponse { Email = targetPermission.UserEmail, Id = targetPermission.UserId }));

            var permission = await _permService.AddUserPermission(targetPermission, currentUserPermission.UserId);

            Assert.NotNull(permission);
            Assert.Equal(targetPermission.Role, permission.Role);
        }

        [Fact]
        public async Task AddUserPermission_ShouldFail_WhenCurrentUserIsNotOwner()
        {
            var targetPermission = new Permission { Role = "Editor", UserId = "AddedUserId", UserEmail = "AddedUserEmail", Room = "roomId" };
            var currentUserPermission = new Permission { Role = "Editor", UserId = "OwnerUserId", UserEmail = "OwnerUserEmail", Room = "roomId" };

            _permRepo.Setup(r => r.GetUserPermissionAsync(currentUserPermission.UserId, "roomId", true))
                .ReturnsAsync(currentUserPermission);

            _permRepo.Setup(r => r.GetUserPermissionAsync(targetPermission.UserId, "roomid", false))
                .ReturnsAsync((Permission?)null);

            _permRepo.Setup(r => r.AddUserPermissionAsync(targetPermission))
                .ReturnsAsync(targetPermission);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _permService.AddUserPermission(targetPermission, currentUserPermission.UserId));
        }

        [Fact]
        public async Task AddUserPermission_ShouldCreateAsOwner_WhenNewRoom()
        {
            var permission = new Permission { Role = "AnyRole", Room = "NewRoom", UserEmail = "UserEmail", UserId = "UserId" };

            _permRepo.Setup(r => r.GetUserPermissionAsync(permission.UserId, permission.Room, true))
                .ReturnsAsync((Permission?)null);

            _permRepo.Setup(r => r.GetOwnerPermissionAsync(permission.Room))
                .ReturnsAsync((Permission?)null);

            _permRepo.Setup(r => r.AddUserPermissionAsync(It.Is<Permission>(p =>
                p.UserId == permission.UserId &&
                p.Room == permission.Room &&
                p.UserEmail == permission.UserEmail &&
                p.Role == "Owner"
            ))).ReturnsAsync((Permission p) => p);

            var result = await _permService.AddUserPermission(permission, permission.UserId);

            Assert.NotNull(result);
            Assert.Equal("Owner", result.Role);
        }

        [Fact]
        public async Task AddUserPermission_ShouldFail_WhenUserAlreadyHasPermission()
        {
            var permission = new Permission { Role = "Owner", Room = "SameRoom", UserEmail = "UserEmail", UserId = "UserId" };

            _permRepo.Setup(r => r.GetUserPermissionAsync(permission.UserId, permission.Room, true))
                .ReturnsAsync(permission);

            _permRepo.Setup(r => r.GetUserPermissionAsync(permission.UserId, permission.Room, false))
                .ReturnsAsync(permission);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _permService.AddUserPermission(permission, permission.UserId));
        }

        [Fact]
        public async Task AddUserPermission_ShouldFail_WhenOwnerCreationFails()
        {
            var perm = new Permission { Room = "room1", UserId = "u1", UserEmail = "a@b.com" };

            _permRepo.Setup(r => r.GetUserPermissionAsync("currentUser", "room1", true))
                .ReturnsAsync((Permission)null);

            _permRepo.Setup(r => r.GetOwnerPermissionAsync("room1"))
                .ReturnsAsync((Permission)null);

            _permRepo.Setup(r => r.AddUserPermissionAsync(It.IsAny<Permission>()))
                .ReturnsAsync((Permission)null);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _permService.AddUserPermission(perm, "currentUser"));
        }

        [Fact]
        public async Task AddUserPermission_ShouldFail_WhenUserDoesNotExist()
        {
            var perm = new Permission { Room = "room1", UserId = "u2", UserEmail = "missing@b.com" };

            _permRepo.Setup(r => r.GetUserPermissionAsync("currentUser", "room1", true))
                .ReturnsAsync(new Permission { UserId = "currentUser", Role = "Owner", Room = "room1" });

            _permRepo.Setup(r => r.GetUserPermissionAsync("u2", "room1", false))
                .ReturnsAsync((Permission)null);

            _userClient.Setup(c => c.EmailToUidAsync(It.IsAny<EmailToUidRequest>(), null, null, default))
                .Throws(new RpcException(new Status(StatusCode.NotFound, "not found")));

            await Assert.ThrowsAsync<InvalidOperationException>(() => _permService.AddUserPermission(perm, "currentUser"));
        }

        /*
        [Theory]
        [InlineData("user-id", "room-id", "Owner", "user-email@gmail.com", "not-same-user-id", false, false)]
        [InlineData("user-id", "room-id", "Owner", "user-email@gmail.com", "user-id", true, false)]
        [InlineData("user-id", "room-id", "Owner", "user-email@gmail.com", "user-id", true, true)]

        public async Task AddUserPermission_Behavior(string userId, string room, string role, string userEmail, string currentUserId, bool shouldSucceed, bool newRoom)
        {
            Permission perm = new Permission { Role = role, Room = room, UserEmail = userEmail, UserId = userId };
            if (newRoom)
            {
                _permRepo.Setup(r => r.GetUserPermissionAsync(currentUserId, room, true)).ReturnsAsync((Permission?)null);
                _permRepo.Setup(r => r.GetOwnerPermissionAsync(room)).ReturnsAsync(perm);
                _permRepo.Setup(r => r.AddUserPermissionAsync(perm)).ReturnsAsync(perm);
            }
            else
            {
                _permRepo.Setup(r => r.GetUserPermissionAsync(currentUserId, room, true))
                    .ReturnsAsync(new Permission { Role = role, Room = room, UserEmail = userEmail, UserId = userId });

                _permRepo.Setup(r => r.GetUserPermissionAsync(userId, room, true)).ReturnsAsync((Permission?)null);

                _userClient.Setup(c => c.EmailToUid(It.IsAny<EmailToUidRequest>(), null, null, default))
                    .Returns(new UserResponse { Email = userEmail, Id = userId });

                _permRepo.Setup(r => r.AddUserPermissionAsync(perm)).ReturnsAsync(perm);
            }

            if (shouldSucceed)
            {
                var result = await _permService.AddUserPermission(perm, currentUserId);
                Assert.NotNull(result);
                Assert.Equal(perm.Role, result.Role);

                if (newRoom)
                {
                    Assert.Equal(currentUserId, result.UserId);
                } else
                {
                    Assert.NotEqual(currentUserId, result.UserId);
                }
            }
            else
            {
                _permService.AddUserPermission(perm, currentUserId);
            }
        }*/
    }
}