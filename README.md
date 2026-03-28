# IdentityProvider

A .NET 10 Web API for user authentication, registration, and JWT-based authorization, backed by a background Worker service for async event processing and email delivery.

## Features

- **User registration** — Create new user accounts with email and optional phone number
- **Email OTP verification** — 6-digit OTP sent via email to activate new accounts (10-minute expiry)
- **Login** — Authenticate users and receive JWT and refresh tokens with per-device session management
- **Login session management** — Track user sessions with device ID, user agent, and IP address
- **Refresh token** — Exchange a valid refresh token for new JWT and refresh tokens
- **Logout** — Deactivate login session, revoke refresh token, and blacklist the JWT
- **JWT authentication** — Bearer token validation with per-request session and revocation checks on protected endpoints
- **Password hashing** — Argon2 for secure password storage
- **Token revocation** — SHA256-hashed JWT blacklisting for enhanced security
- **Background Worker** — Azure Service Bus consumer that handles async tasks (e.g., sending registration OTP emails via SMTP)

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

## Project Structure

| Project | Description |
|--------|-------------|
| `IdentityProvider.WebAPI` | ASP.NET Core API host — controllers, JWT middleware, Swagger |
| `Worker` | .NET Background Service — Azure Service Bus consumer, email dispatch via MailKit |
| `Services` | Business logic — authentication, repositories (user, OTP, refresh token, login session, revoked token), token generation, Service Bus publisher |
| `Models` | Domain models, DTOs, data models, events, enums |
| `DatabaseAdapter` | Dapper-based data access layer with manual transaction support |
| `Extensions` | AutoMapper factory, audit field helpers, JSON and string extensions |
| `Encryption` | Argon2 password hashing, SHA256 token hashing |
| `Database` | SQL Server schema project — DDL for all tables |

## API Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/user/registration` | — | Register a new user; sends OTP email |
| `POST` | `/api/user/registration/verify-otp` | — | Verify registration OTP and activate account |
| `POST` | `/api/login` | — | Login and receive JWT + refresh tokens (creates a login session) |
| `POST` | `/api/token/refresh` | Bearer | Exchange current token pair for a new JWT + refresh token |
| `POST` | `/api/logout` | Bearer | Deactivate session, revoke refresh token, blacklist JWT |
| `GET` | `/api/authentication` | Bearer | Validate authentication and session status |
| `GET` | `/api/api-test/test-service-bus` | — | Send a test event to Service Bus (dev/test only) |
| `GET` | `/api/api-test/test-service-bus-delayed` | — | Send a test event scheduled 10 seconds in the future (dev/test only) |

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

### Login — `POST /api/login`

```json
{
  "email": "string",
  "password": "string",
  "deviceId": "guid"
}
```

`deviceId` is required for per-device session tracking.

### Token Response

`POST /api/login` and `POST /api/token/refresh` both return:

```json
{
  "jwtToken": "string",
  "refreshToken": "string"
}
```

### Logout / Refresh — `POST /api/logout` and `POST /api/token/refresh`

```json
{
  "userId": "guid",
  "sessionId": "guid"
}
```

**Note:** The JWT includes a session ID (`sid` claim) that links to the active `LoginSession`. On every authenticated request, the middleware SHA256-hashes the incoming JWT and checks the `RevokedToken` table, then verifies the referenced `LoginSession` is still active.

## Database Schema

| Table | Purpose |
|-------|---------|
| `User` | User accounts — stores name, email, hashed password, optional phone, `AccountStatus` (`0=NotVerified`, `1=Active`, `2=Inactive`, `3=LockedOut`) |
| `UserOtp` | Short-lived 6-digit OTPs for email verification (10-minute expiry, single-use) |
| `LoginSession` | Per-device sessions — tracks `DeviceId`, `UserAgent`, `IpAddress`, `IsActive`, `LastSeenOn` |
| `RefreshToken` | Active refresh tokens per session — filtered unique index enforces one active token per `(UserId, SessionId)` |
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
