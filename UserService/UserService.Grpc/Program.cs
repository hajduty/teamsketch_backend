using UserService.Grpc.Services;
using UserService.Infrastructure;

namespace UserService.Grpc
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var config = builder.Configuration;

            // Add services to the container.
            builder.Services.AddGrpc();
            builder.Services.AddInfrastructure(config);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.MapGrpcService<GrpcService>();
            app.MapGet("/", () => "gRPC Service is running");

            app.Run();
        }
    }
}