# TeamSketch Backend

[![CI](https://github.com/hajduty/teamsketch_backend/actions/workflows/ci.yml/badge.svg)](https://github.com/hajduty/teamsketch_backend/actions/workflows/ci.yml)

Backend for **TeamSketch**, a real-time collaborative whiteboard. Users sign up, create rooms, invite others as editors or viewers, and draw together live.

[Blog post](https://www.hajder.app/projects/teamsketch) · [Frontend repo](https://github.com/hajduty/teamsketch_frontend)

## Architecture

<img width="1823" height="785" alt="Overview of the TeamSketch services" src="https://github.com/user-attachments/assets/309a217e-1f48-4aa9-87fb-81ea23f8f1d9" />

| Service | Stack | Responsibility |
|---|---|---|
| **PermissionService** | ASP.NET Core, EF Core, SignalR | Room ownership and roles (owner / editor / viewer). REST for the frontend, gRPC for RoomService, SignalR to push permission changes to clients. |
| **RoomService** | Node.js, [Yjs](https://yjs.dev) | Real-time document sync over WebSocket, using [y/hub (y-redis)](https://github.com/yjs/y-redis). |
| **AuthService** | ASP.NET Core | Issues RS256 JWTs and publishes the public key at `/.well-known/jwks.json`. |
| **UserService** | ASP.NET Core gRPC, EF Core | Accounts and password hashing. Internal only, reachable over gRPC. |

The contracts between services live in [`Shared/Contracts/Protos`](Shared/Contracts/Protos).

## Running locally

Requires Docker.

```bash
git clone https://github.com/hajduty/teamsketch_backend.git
cd teamsketch_backend
docker compose up --build
```

| | URL |
|---|---|
| AuthService | http://localhost:7154 |
| PermissionService (REST / SignalR) | http://localhost:7100 |
| RoomService (WebSocket) | ws://localhost:3002 |

Then run the [frontend](https://github.com/hajduty/teamsketch_frontend) on `http://localhost:5173`.

## Testing

| Suite | What it covers | Runs on |
|---|---|---|
| `*/…Tests` | Unit tests for each .NET service (xUnit, Moq). | `dotnet test` |
| `E2E.Tests` (in-process) | Auth, User and Permission services hosted together with `WebApplicationFactory` and in-memory databases. | `dotnet test` |
| `E2E.Tests` (Docker) | The full system in containers through [Testcontainers](https://dotnet.testcontainers.org/): MySQL, Redis and every service image. Includes registering, creating a room, sharing it, connecting over WebSocket, and checking that a revoked user gets disconnected. | `docker compose build && dotnet test` |
| `RoomService/tests` | The storage contract (memory and MySQL) and the Redis stream API. | `npm test` with `REDIS` and `MYSQL` set |

GitHub Actions runs all of these on every pull request.

## Deployment

Azure Kubernetes Service, defined in [`infra`](infra):

- [`infra/modules`](infra/modules): Bicep for AKS, Container Registry and MySQL Flexible Server
- [`infra/charts`](infra/charts): Helm chart with ingress, TLS and autoscaling
- [`azure-pipelines.yml`](azure-pipelines.yml): builds the images, runs migrations and deploys the chart
