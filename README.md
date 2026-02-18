# IdentityProvider

A .NET Web API for user authentication, registration, and JWT-based authorization.

## Features

- **User registration** — Create new user accounts with email and password
- **Login** — Authenticate users and receive JWT and refresh tokens with session management
- **Login session management** — Track user sessions with device information, user agent, and IP address
- **Refresh token** — Exchange a valid refresh token for new JWT and refresh tokens
- **Logout** — Deactivate login session, revoke refresh token, and blacklist the JWT
- **JWT authentication** — Bearer token validation with session validation for protected endpoints
- **Password hashing** — Argon2 for secure password storage
- **Token revocation** — SHA256-hashed token blacklisting for enhanced security

## Tech Stack

- .NET 10.0
- ASP.NET Core Web API
- JWT Bearer authentication with refresh tokens
- Dapper for data access
- SQL Server database
- Azure Key Vault for secrets (optional)
- Swagger/OpenAPI (development)

## Project Structure

| Project | Description |
|--------|-------------|
| IdentityProvider.WebAPI | ASP.NET Core API with controllers and JWT setup |
| Services | Business logic, authentication, repositories (user, refresh token, login session, revoked token), token generation |
| Models | Domain models, DTOs, data models |
| DatabaseAdapter | Dapper-based data access layer |
| Extensions | Mapping, JSON, and audit field extensions |
| Encryption | Argon2 password hashing, SHA256 token hashing |
| Database | SQL Server schema and tables (User, RefreshToken, LoginSession, RevokedToken) |

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/user/registration` | Register a new user |
| POST | `/api/login` | Login and receive JWT and refresh tokens (creates a login session) |
| POST | `/api/token/refresh` | Exchange current JWT for new JWT and refresh token (requires Bearer token, validates session) |
| POST | `/api/logout` | Logout, deactivate session, and revoke tokens (requires Bearer token) |
| GET | `/api/authentication` | Validate authentication and session (requires Bearer token) |

### Registration Request Body

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

(`middleName`, `countryCode`, and `mobileNumber` are optional.)

### Login Request Body

```json
{
  "email": "string",
  "password": "string",
  "deviceId": "guid",
  "deviceName": "string"
}
```

(`deviceId` and `deviceName` are required for session tracking.)

### Login and Refresh Token Response

`POST /api/login` and `POST /api/token/refresh` return:

```json
{
  "jwtToken": "string",
  "refreshToken": "string"
}
```

**Note:** The JWT token includes a session ID (`sid` claim) that links to the login session. On login, a new session is created with device information, user agent, and IP address. Token validation checks that the session is active and belongs to the authenticated user.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (or Azure SQL Database)
- Azure Key Vault (optional) — for storing JWT key and connection string in production

## Configuration

Configure `appsettings.json` with your settings:

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
  "AzureKeyVaultUri": "https://your-keyvault.vault.azure.net/"
}
```

- **Azure Key Vault** — The app loads secrets from Azure Key Vault using `AzureKeyVaultUri` and DefaultAzureCredential. Store `Key` and `IdentityDatabaseConnectionString` in Key Vault, or set them in `appsettings.Development.json` / environment variables for local development (ensure `AzureKeyVaultUri` points to a valid vault or the app may fail at startup).
- For local development without Key Vault, use `appsettings.Development.json` or environment variables for the connection string and JWT key.

## Getting Started

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build

# Run
dotnet run --project IdentityProvider.WebAPI
```

The API runs at **https://localhost:7285** (HTTP: **http://localhost:5281**) by default. See `IdentityProvider.WebAPI/Properties/launchSettings.json` for other profiles. In Development mode, Swagger UI is served at the root URL with Bearer (JWT) support configured.

## License

Private/Unspecified
