using AuthService.Core.DTOs;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Net.Http.Json;
using Testcontainers.MsSql;
using Testcontainers.Redis;
using Xunit.Abstractions;

namespace E2E.Tests.Fixtures
{
    public class DockerServerFixture : IAsyncLifetime
    {
        public MsSqlContainer Sql { get; private set; }
        public RedisContainer Redis { get; private set; }
        public IContainer RoomService { get; private set; }
        public IContainer UserService { get; private set; }
        public IContainer AuthService { get; private set; }
        public IContainer PermissionService { get; private set; }
        private INetwork _network;

        public async Task InitializeAsync()
        {
            Debug.WriteLine("Creating network...");
            _network = new NetworkBuilder()
                .Build();

            await _network.CreateAsync();
            Debug.WriteLine("Network created.");

            var sqlDbScript = Path.GetFullPath("create_database.sql");
            var sqlTableScript = Path.GetFullPath("create_tables.sql");

            if (!File.Exists(sqlDbScript) || !File.Exists(sqlTableScript))
            {
                throw new FileNotFoundException($"Script not found at {sqlDbScript}");
            }

            Sql = new MsSqlBuilder()
                .WithNetwork(_network)
                .WithNetworkAliases("sqlserver")
                .WithPassword("YourStrong(!)Password")
                .WithBindMount(sqlDbScript, "/sqlDbScript.sql")
                .WithBindMount(sqlTableScript, "/sqlTableScript.sql")
                .WithEnvironment("MSSQL_SA_PASSWORD", "YourStrong(!)Password")
                .WithEnvironment("MSSQL_PID", "Developer")
                .WithEnvironment("ACCEPT_EULA", "Y")
                .Build();

            await Sql.StartAsync();

            var mappedPort = Sql.GetMappedPublicPort(1433);
            using var conn = new SqlConnection($"Server=127.0.0.1,{mappedPort};Database=master;User Id=sa;Password=YourStrong(!)Password;TrustServerCertificate=True;");

            for (int i = 0; i < 30; i++)
            {
                try
                {
                    await conn.OpenAsync();
                    break;
                }
                catch
                {
                    if (i == 29) throw;
                    await Task.Delay(1000);
                }
            }

            var dbInit = await File.ReadAllTextAsync(sqlDbScript);
            using (var initCmd = new SqlCommand(dbInit, conn))
            {
                await initCmd.ExecuteNonQueryAsync();
            }

            using var tsConn = new SqlConnection($"Server=127.0.0.1,{mappedPort};Database=TS;User Id=sa;Password=YourStrong(!)Password;TrustServerCertificate=True;");

            await tsConn.OpenAsync();

            var tableInit = await File.ReadAllTextAsync(sqlTableScript);
            using (var initCmd = new SqlCommand(tableInit, tsConn))
            {
                await initCmd.ExecuteNonQueryAsync();
            }

            if (conn.State != ConnectionState.Closed)
                await conn.CloseAsync();

            if (tsConn.State != ConnectionState.Closed)
                await tsConn.CloseAsync();

            Redis = new RedisBuilder()
                .WithNetwork(_network)
                .WithNetworkAliases("redis")
                .Build();

            await Redis.StartAsync();

            // Now build and start services with the actual connection strings
            // Use container names for internal communication within the network
            UserService = new ContainerBuilder()
                .WithImage("webapplication4-userservice:latest")
                .WithNetwork(_network)
                .WithNetworkAliases("userservice")
                .WithEnvironment("ConnectionStrings__DefaultConnection", "Server=sqlserver,1433;Database=TS;User Id=sa;Password=YourStrong(!)Password;TrustServerCertificate=True;")
                .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Test")
                .WithPortBinding(8080, 8080)
                .WithWaitStrategy(Wait.ForUnixContainer()
                    .UntilMessageIsLogged("Now listening on"))
                .Build();

            await UserService.StartAsync();

            AuthService = new ContainerBuilder()
                .WithImage("webapplication4-authservice:latest")
                .WithNetwork(_network)
                .WithNetworkAliases("authservice")
                .WithEnvironment("ConnectionStrings__DefaultConnection", "Server=sqlserver,1433;Database=TS;User Id=sa;Password=YourStrong(!)Password;TrustServerCertificate=True;")
                .WithEnvironment("UserServiceURL", "http://userservice:8080")
                .WithPortBinding(7154, 8080)
                .WithWaitStrategy(Wait.ForUnixContainer()
                    .UntilMessageIsLogged("Now listening on")
                    .UntilHttpRequestIsSucceeded(r => r
                        .ForPath("/.well-known/jwks.json")
                        .ForPort(8080)))
                .Build();

            await AuthService.StartAsync();

            PermissionService = new ContainerBuilder()
                .WithImage("webapplication4-permissionservice:latest")
                .WithNetwork(_network)
                .WithNetworkAliases("permissionservice")
                .WithEnvironment("ConnectionStrings__DefaultConnection", "Server=sqlserver,1433;Database=TS;User Id=sa;Password=YourStrong(!)Password;TrustServerCertificate=True;")
                .WithEnvironment("AuthServiceURL", "http://authservice:8080")
                .WithEnvironment("UserServiceURL", "http://userservice:8080")
                .WithEnvironment("ConnectionStrings__RedisConnectionString", "redis:6379,abortConnect=false")
                .WithEnvironment("CERT_PATH", "Shared/Certs/server.pfx")
                .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Test")
                .WithEnvironment("ASPNETCORE_URLS", "http://0.0.0.0:7122")
                .WithPortBinding(7122, 7122)
                .WithPortBinding(7100, 7100)
                .WithWaitStrategy(Wait.ForUnixContainer()
                    .UntilMessageIsLogged("Now listening on"))
                .Build();

            await PermissionService.StartAsync();

            RoomService = new ContainerBuilder()
                .WithImage("webapplication4-roomservice:latest")
                .WithNetwork(_network)
                .WithNetworkAliases("roomservice")
                .WithEnvironment("ConnectionStrings__DefaultConnection", "Server=sqlserver,1433;Database=TS;User Id=sa;Password=YourStrong(!)Password;TrustServerCertificate=True;")
                .WithEnvironment("PROTO_PATH", "Shared/Contracts/Protos/permission_service.proto")
                .WithEnvironment("CERT_PATH", "Shared/Certs/server.crt")
                .WithEnvironment("REDIS_URL", "redis:6379")
                .WithEnvironment("PERMISSION_SERVICE_URL", "permissionservice:7122")
                .WithPortBinding(1234, 1234)
                .WithWaitStrategy(Wait.ForUnixContainer()
                    .UntilMessageIsLogged("Redis ready"))
                .Build();

            await RoomService.StartAsync();
        }

        public async Task DisposeAsync()
        {
            var stopTasks = new List<Task>();

            if (RoomService != null)
                stopTasks.Add(RoomService.DisposeAsync().AsTask());
            if (PermissionService != null)
                stopTasks.Add(PermissionService.DisposeAsync().AsTask());
            if (AuthService != null)
                stopTasks.Add(AuthService.DisposeAsync().AsTask());
            if (UserService != null)
                stopTasks.Add(UserService.DisposeAsync().AsTask());

            if (Redis != null)
                stopTasks.Add(Redis.DisposeAsync().AsTask());
            if (Sql != null)
                stopTasks.Add(Sql.DisposeAsync().AsTask());

            await Task.WhenAll(stopTasks);

            if (_network != null)
            {
                await _network.DisposeAsync();
            }
        }
    }
}