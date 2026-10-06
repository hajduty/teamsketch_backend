using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using Testcontainers.MySql;
using Testcontainers.Redis;

namespace E2E.Tests.Fixtures
{
    /// <summary>
    /// Runs the whole backend as containers (images from `docker compose build`) on a private network,
    /// with MySQL and Redis next to them. Host ports are random, use GetMappedPublicPort.
    /// </summary>
    public class DockerServerFixture : IAsyncLifetime
    {
        private const string MySqlPassword = "E2eTestPassword123";
        private const string DbConnection = $"Server=mysql;Port=3306;Database=TS;User ID=root;Password={MySqlPassword}";
        private const string RoomStoreUrl = $"mysql://root:{MySqlPassword}@mysql:3306/TS";

        public MySqlContainer Sql { get; private set; } = null!;
        public RedisContainer Redis { get; private set; } = null!;
        public IContainer UserService { get; private set; } = null!;
        public IContainer AuthService { get; private set; } = null!;
        public IContainer PermissionService { get; private set; } = null!;
        public IContainer RoomService { get; private set; } = null!;
        public IContainer RoomWorker { get; private set; } = null!;
        private INetwork _network = null!;

        public async Task InitializeAsync()
        {
            _network = new NetworkBuilder().Build();
            await _network.CreateAsync();

            Sql = new MySqlBuilder()
                .WithImage("mysql:8.0")
                .WithNetwork(_network)
                .WithNetworkAliases("mysql")
                .WithUsername("root")
                .WithPassword(MySqlPassword)
                .WithDatabase("TS")
                .Build();

            Redis = new RedisBuilder()
                .WithImage("redis:7-alpine")
                .WithNetwork(_network)
                .WithNetworkAliases("redis")
                .Build();

            await Task.WhenAll(Sql.StartAsync(), Redis.StartAsync());

            var tableScript = await File.ReadAllTextAsync(Path.GetFullPath("02_create_tables.sql"));
            var scriptResult = await Sql.ExecScriptAsync(tableScript);
            if (scriptResult.ExitCode != 0)
                throw new InvalidOperationException($"Creating tables failed: {scriptResult.Stderr}");

            UserService = new ContainerBuilder()
                .WithImage("teamsketch_backend-userservice:latest")
                .WithNetwork(_network)
                .WithNetworkAliases("userservice")
                .WithEnvironment("ConnectionStrings__DefaultConnection", DbConnection)
                .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Test")
                .WithPortBinding(8080, true)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("Now listening on"))
                .Build();

            await UserService.StartAsync();

            AuthService = new ContainerBuilder()
                .WithImage("teamsketch_backend-authservice:latest")
                .WithNetwork(_network)
                .WithNetworkAliases("authservice")
                .WithEnvironment("UserServiceURL", "http://userservice:8080")
                .WithPortBinding(8080, true)
                .WithWaitStrategy(Wait.ForUnixContainer()
                    .UntilHttpRequestIsSucceeded(r => r.ForPath("/.well-known/jwks.json").ForPort(8080)))
                .Build();

            await AuthService.StartAsync();

            PermissionService = new ContainerBuilder()
                .WithImage("teamsketch_backend-permissionservice:latest")
                .WithNetwork(_network)
                .WithNetworkAliases("permissionservice")
                .WithEnvironment("ConnectionStrings__DefaultConnection", DbConnection)
                .WithEnvironment("ConnectionStrings__RedisConnectionString", "redis:6379,abortConnect=false")
                .WithEnvironment("AuthServiceURL", "http://authservice:8080")
                .WithEnvironment("UserServiceURL", "http://userservice:8080")
                .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Test")
                .WithPortBinding(7122, true)
                .WithPortBinding(7100, true)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("Now listening on"))
                .Build();

            await PermissionService.StartAsync();

            RoomService = new ContainerBuilder()
                .WithImage("teamsketch_backend-roomservice:latest")
                .WithNetwork(_network)
                .WithNetworkAliases("roomservice")
                .WithCommand("node", "./bin/server.js", "--no-colors")
                .WithEnvironment("PORT", "3002")
                .WithEnvironment("PROTO_PATH", "Shared/Contracts/Protos/permission_service.proto")
                .WithEnvironment("PERMISSION_SERVICE_URL", "permissionservice:7122")
                .WithEnvironment("REDIS", "redis://redis:6379")
                .WithEnvironment("REDIS_URL", "redis:6379")
                .WithEnvironment("MYSQL", RoomStoreUrl)
                .WithPortBinding(3002, true)
                .WithWaitStrategy(Wait.ForUnixContainer()
                    .UntilHttpRequestIsSucceeded(r => r.ForPath("/healthz").ForPort(3002)))
                .Build();

            RoomWorker = new ContainerBuilder()
                .WithImage("teamsketch_backend-roomservice:latest")
                .WithNetwork(_network)
                .WithCommand("node", "./bin/worker.js", "--no-colors")
                .WithEnvironment("REDIS", "redis://redis:6379")
                .WithEnvironment("MYSQL", RoomStoreUrl)
                .Build();

            await Task.WhenAll(RoomService.StartAsync(), RoomWorker.StartAsync());
        }

        public async Task DisposeAsync()
        {
            var containers = new IAsyncDisposable?[] { RoomWorker, RoomService, PermissionService, AuthService, UserService, Redis, Sql };
            await Task.WhenAll(containers.Where(c => c != null).Select(c => c!.DisposeAsync().AsTask()));

            if (_network != null)
                await _network.DisposeAsync();
        }
    }
}
