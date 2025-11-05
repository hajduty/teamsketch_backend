# Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0.305-bookworm-slim AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Install protobuf compiler
RUN apt-get update && \
    apt-get install -y --no-install-recommends protobuf-compiler && \
    rm -rf /var/lib/apt/lists/*
ENV PROTOBUF_PROTOC=/usr/bin/protoc

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0.305-bookworm-slim AS build
WORKDIR /src

# Copy API + other projects that belong to this service
COPY UserService/ UserService/
COPY Shared/ Shared/

# Restore dependencies
RUN dotnet restore "UserService/UserService.Grpc/UserService.Grpc.csproj"

# Copy the rest of the source code
COPY . .

# Build
RUN dotnet build "UserService/UserService.Grpc/UserService.Grpc.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "UserService/UserService.Grpc/UserService.Grpc.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY Shared/ Shared/
ENTRYPOINT ["dotnet", "UserService.Grpc.dll"]
