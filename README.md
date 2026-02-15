# IdentityProvider

A .NET Web API for user authentication, registration, and JWT-based authorization.

## Features

- **User registration** — Create new user accounts with email and password
- **Login** — Authenticate users and receive JWT and refresh tokens
- **Refresh token** — Exchange a valid refresh token for new JWT and refresh tokens
- **Logout** — Revoke the current refresh token and blacklist the JWT
- **JWT authentication** — Bearer token validation for protected endpoints
- **Password hashing** — Argon2 for secure password storage

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
| Services | Business logic, authentication, repositories, token generation |
| Models | Domain models, DTOs, data models |
| DatabaseAdapter | Dapper-based data access layer |
| Extensions | Mapping, JSON, and audit field extensions |
| Encryption | Argon2 password hashing, SHA256 token hashing |
| Database | SQL Server schema and tables |

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/user/registration` | Register a new user |
| POST | `/api/login` | Login and receive JWT and refresh tokens |
| POST | `/api/token/refresh` | Exchange current JWT for new JWT and refresh token (requires Bearer token) |
| POST | `/api/logout` | Logout and revoke tokens (requires Bearer token) |
| GET | `/api/authentication` | Validate authentication (requires Bearer token) |

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
  "password": "string"
}
```

### Login and Refresh Token Response

`POST /api/login` and `POST /api/token/refresh` return:

```json
{
  "jwtToken": "string",
  "refreshToken": "string"
}
```

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

- **Azure Key Vault** — When `AzureKeyVaultUri` is set, the app loads secrets from Azure Key Vault (e.g. `Key`, `IdentityDatabaseConnectionString`) using DefaultAzureCredential. Omit or override in local config if not using Key Vault.
- For local development, use `appsettings.Development.json` or environment variables to override sensitive values.

## Getting Started

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build

# Run
dotnet run --project IdentityProvider.WebAPI
```

The API runs at `https://localhost:5001` (or the port in `launchSettings.json`). Swagger UI is available at the root URL in Development mode.

## License

Private/Unspecified
