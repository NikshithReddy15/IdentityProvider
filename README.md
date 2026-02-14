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
- JWT Bearer authentication
- Dapper for data access
- SQL Server database
- Swagger/OpenAPI (development)

## Project Structure

| Project | Description |
|--------|-------------|
| IdentityProvider.WebAPI | ASP.NET Core API with controllers and JWT setup |
| Services | Business logic, authentication, repositories, token generation |
| Models | Domain models, DTOs, data models |
| DatabaseAdapter | Dapper-based data access layer |
| Extensions | Mapping, JSON, and audit field extensions |
| Encryption | Argon2 hashing service |
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
  "firstname": "string",
  "middlename": "string",
  "lastname": "string",
  "email": "string",
  "countryCode": "string",
  "mobileNumber": "string",
  "password": "string"
}
```

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

## Configuration

Configure `appsettings.json` with your settings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "your-sql-server-connection-string"
  },
  "JwtSettings": {
    "Issuer": "AuthenticationAPIServer",
    "Audience": "AuthenticationAPIUser",
    "Key": "your-base64-encoded-secret-key",
    "ExpiryInMinutes": 60,
    "RefreshTokenExpiryInDays": 30
  }
}
```

For local development, use `appsettings.Development.json` or environment variables to override sensitive values.

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
