# IdentityProvider

A .NET 10 Web API for user authentication, registration, and JWT-based authorization, backed by a background Worker service for async event processing and email delivery.

## Features

- **User registration** — Create new user accounts with email and optional phone number
- **Email OTP verification** — 6-digit OTP sent via email to activate new accounts (10-minute expiry), with resend support
- **Login** — Authenticate users and receive JWT and refresh tokens with per-device session management
- **Login session management** — Track user sessions with device ID, user agent, and IP address
- **Refresh token** — Exchange a valid refresh token for new JWT and refresh tokens
- **Logout** — Deactivate login session, revoke refresh token, and blacklist the JWT
- **JWT authentication** — Bearer token validation with per-request session and revocation checks on protected endpoints
- **Password hashing** — Argon2 for secure password storage
- **Token revocation** — SHA256-hashed JWT blacklisting for enhanced security
- **Background Worker** — Azure Service Bus consumer that handles async tasks (e.g., sending registration OTP emails via SMTP)
- **Recurring Jobs** — Hangfire-powered scheduled jobs; cleans up expired/used OTPs every 10 minutes; bulk-revokes expired refresh tokens, deactivates their sessions, and purges expired JWT blacklist entries every hour
- **Timed Events** — Token expiry events published to Service Bus; Worker receives them and fires in-process timed tasks that execute at exact expiry times to revoke refresh tokens, deactivate login sessions, and clean up the JWT blacklist

## Tech Stack

- .NET 10.0
- ASP.NET Core Web API
- JWT Bearer authentication (HMAC-SHA256) with refresh tokens
- Dapper for data access
- SQL Server database
- Azure Service Bus for async event messaging
- MailKit for SMTP email delivery
- Argon2 password hashing (`Soenneker.Hashing.Argon2`)
- AutoMapper for object mapping
- Azure Key Vault for secrets management (optional)
- Swagger/OpenAPI (development only)
- Hangfire for recurring and delayed background jobs (SQL Server storage)

## Project Structure

| Project | Description |
|--------|-------------|
| `IdentityProvider.WebAPI` | ASP.NET Core API host — controllers, JWT middleware, Swagger |
| `Worker` | .NET Background Service — Azure Service Bus consumer, email dispatch via MailKit, Hangfire server (recurring: OTP cleanup every 10 min, token expiry sweep every hour; timed: per-token in-process tasks) |
| `Services` | Business logic — authentication, repositories (user, OTP, refresh token, login session, revoked token), token generation, Service Bus publisher |
| `Models` | Domain models, DTOs, data models, events, enums |
| `DatabaseAdapter` | Dapper-based data access layer with manual transaction support |
| `Extensions` | AutoMapper factory, audit field helpers, JSON and string extensions |
| `Encryption` | Argon2 password hashing, SHA256 token hashing |
| `Database` | SQL Server schema project — DDL for all tables |
| `IdentityProvider.Auth` | Reusable NuGet package — JWT Bearer validation middleware for other microservices (see below) |

## IdentityProvider.Auth NuGet Package

`IdentityProvider.Auth` is an internal SDK for other microservices that need to validate JWTs issued by this IdentityProvider. It centralises the HS256 bearer validation logic so consumer services don't duplicate JWT configuration or share secrets insecurely.

### Installation

Reference the built `.nupkg` from `IdentityProvider.Auth/bin/Debug/IdentityProvider.Auth.1.0.0.nupkg` via a local NuGet feed, or publish it to a private feed.

### Usage

```csharp
// Program.cs of the consuming microservice
builder.Services.AddIdentityProviderAuthentication(options =>
{
    options.Key      = config["JwtSettings:Key"];      // HS256 signing key (load from Key Vault)
    options.Issuer   = config["JwtSettings:Issuer"];
    options.Audience = config["JwtSettings:Audience"];

    // Optional: validate each request against IdentityProvider's /api/authentication endpoint
    options.EnableRemoteRevocationCheck  = true;
    options.IdentityProviderBaseUrl      = config["IdentityProviderBaseUrl"];
});

// Still required in the consuming service:
builder.Services.AddAuthorization();
app.UseAuthentication();
app.UseAuthorization();
```

### Options

| Option | Required | Default | Description |
|--------|----------|---------|-------------|
| `Key` | Yes | — | HS256 signing key (must match the key used by IdentityProvider) |
| `Issuer` | Yes | — | Expected JWT issuer (`JwtSettings:Issuer`) |
| `Audience` | Yes | — | Expected JWT audience (`JwtSettings:Audience`) |
| `EnableRemoteRevocationCheck` | No | `false` | When `true`, each validated token is verified against `GET /api/authentication` on IdentityProvider; rejects revoked/expired sessions |
| `IdentityProviderBaseUrl` | Conditional | — | Required when `EnableRemoteRevocationCheck` is `true`; base URL of IdentityProvider (e.g. `https://identity.internal`) |

### What it configures

- JWT Bearer authentication with `MapInboundClaims = false` (raw `sub`/`sid` claim names)
- `ValidateIssuer`, `ValidateAudience`, `ValidateLifetime`, `ValidateIssuerSigningKey` all enabled
- Optional per-request revocation check via a named `IHttpClientFactory` client
- Plain-text `401 Unauthorized` / `403 Forbidden` responses on challenge/failure (consistent with IdentityProvider itself)

## API Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/user/registration` | — | Register a new user; sends OTP email |
| `POST` | `/api/user/registration/verify-otp` | — | Verify registration OTP and activate account |
| `POST` | `/api/user/registration/resend-otp` | — | Resend registration OTP to the user's email |
| `POST` | `/api/login` | — | Login and receive JWT + refresh tokens (creates a login session) |
| `POST` | `/api/token/refresh` | Bearer | Exchange current token pair for a new JWT + refresh token |
| `POST` | `/api/logout` | Bearer | Deactivate session, revoke refresh token, blacklist JWT |
| `GET` | `/api/authentication` | Bearer | Validate authentication and session status |

### Registration — `POST /api/user/registration`

```json
{
  "firstName": "string",
  "middleName": "string",
  "lastName": "string",
  "email": "string",
  "countryCode": "string",
  "mobileNumber": "string",
  "password": "string"
}
```

`middleName`, `countryCode`, and `mobileNumber` are optional. Creates the user with `AccountStatus = NotVerified` and sends a 6-digit OTP to the provided email address.

### OTP Verification — `POST /api/user/registration/verify-otp`

```json
{
  "email": "string",
  "otpCode": "string"
}
```

Marks the OTP as used and sets `AccountStatus = Active`.

### Resend OTP — `POST /api/user/registration/resend-otp`

```json
{
  "email": "string"
}
```

Generates a new 6-digit OTP and sends it to the user's email. Only valid while `AccountStatus = NotVerified`.

### Login — `POST /api/login`

```json
{
  "email": "string",
  "password": "string",
  "deviceId": "guid",
  "deviceName": "string"
}
```

`deviceId` and `deviceName` are required for per-device session tracking.

### Token Response

`POST /api/login` and `POST /api/token/refresh` both return:

```json
{
  "jwtToken": "string",
  "refreshToken": "string"
}
```

### Logout / Refresh — `POST /api/logout` and `POST /api/token/refresh`

No request body. `userId` and `sessionId` are extracted server-side from the `sub` and `sid` JWT claims.

**Note:** The JWT includes a session ID (`sid` claim) that links to the active `LoginSession`. On every authenticated request, the middleware SHA256-hashes the incoming JWT and checks the `RevokedToken` table, then verifies the referenced `LoginSession` is still active.

## Database Schema

| Table | Purpose |
|-------|---------|
| `User` | User accounts — stores name, email, hashed password, optional phone, `AccountStatus` (`0=NotVerified`, `1=Active`, `2=Inactive`, `3=LockedOut`) |
| `UserOtp` | Short-lived 6-digit OTPs for email verification (10-minute expiry, single-use) |
| `LoginSession` | Per-device sessions — tracks `DeviceId`, `DeviceName`, `UserAgent`, `IpAddress`, `LastSeenOn`, `DeactivatedOn` (NULL = active); indexed on `UserId` for login history queries |
| `RefreshToken` | Refresh tokens per session — `RevokedOn` (NULL = active); filtered unique index enforces one active token per `(UserId, SessionId)` |
| `RevokedToken` | JWT blacklist — stores SHA256 hash of revoked JWTs with expiry for efficient lookup |

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (or Azure SQL Database)
- Azure Service Bus namespace and queue
- SMTP server (for the Worker email service)
- Azure Key Vault (optional) — recommended for production secrets

## Configuration

### WebAPI (`IdentityProvider.WebAPI/appsettings.json`)

```json
{
  "ConnectionStrings": {
    "IdentityDatabaseConnectionString": "your-sql-server-connection-string"
  },
  "JwtSettings": {
    "Issuer": "IdentityProviderServer",
    "Audience": "IdentityProviderUser",
    "Key": "your-secret-key",
    "ExpiryInMinutes": 60,
    "RefreshTokenExpiryInDays": 30
  },
  "ServiceBus": {
    "ServiceBusConnectionString": "your-service-bus-connection-string",
    "QueueName": "your-queue-name"
  },
  "AzureKeyVaultUri": "https://your-keyvault.vault.azure.net/"
}
```

### Worker (`Worker/appsettings.json`)

```json
{
  "ConnectionStrings": {
    "IdentityDatabaseConnectionString": "your-sql-server-connection-string"
  },
  "ServiceBus": {
    "ServiceBusConnectionString": "your-service-bus-connection-string",
    "QueueName": "your-queue-name"
  },
  "Smtp": {
    "Host": "your-smtp-host",
    "Port": 587,
    "Username": "your-smtp-username",
    "Password": "your-smtp-password",
    "SenderName": "Your App Name",
    "SenderEmail": "no-reply@yourdomain.com"
  },
  "AzureKeyVaultUri": "https://your-keyvault.vault.azure.net/"
}
```

Both hosts load secrets from Azure Key Vault at startup using `DefaultAzureCredential`, overriding placeholder values. For local development without Key Vault, use `appsettings.Development.json` or environment variables.

## Getting Started

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build

# Run the API
dotnet run --project IdentityProvider.WebAPI

# Run the Worker (separate terminal)
dotnet run --project Worker
```

The API runs at **https://localhost:7285** (HTTP: **http://localhost:5281**) by default. See `IdentityProvider.WebAPI/Properties/launchSettings.json` for other profiles. In Development mode, Swagger UI is served at the root URL with Bearer (JWT) support configured.

## License

Private/Unspecified
