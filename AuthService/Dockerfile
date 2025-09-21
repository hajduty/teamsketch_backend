# Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy API + other projects that belong to this service
COPY AuthService/ AuthService/
COPY Shared/ Shared/

# Restore dependencies
RUN dotnet restore "AuthService/AuthService.API/AuthService.API.csproj"

# Copy the rest of the source code
COPY . .

# Build
RUN dotnet build "AuthService/AuthService.API/AuthService.API.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "AuthService/AuthService.API/AuthService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY Shared/ Shared/
ENTRYPOINT ["dotnet", "AuthService.API.dll"]
