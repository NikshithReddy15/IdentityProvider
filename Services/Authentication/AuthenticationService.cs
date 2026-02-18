using DatabaseAdapter.Dapper;
using Encryption.Hashing.Argon2Hashing;
using Encryption.Hashing.SHA256Hashing;
using Extensions;
using Extensions.Mapper;
using Models.CoreModels;
using Models.DataModels;
using Models.DataTransferObjects;
using Services.Repositories.LoginSession;
using Services.Repositories.RefreshToken;
using Services.Repositories.RevokedToken;
using Services.Repositories.User;
using Services.TokenGeneration;

namespace Services.Authentication;

public class AuthenticationService(IUserRepository userRepository, IRevokedTokenRepository revokedTokenRepository, IRefreshTokenRepository refreshTokenRepository, ILoginSessionRepository loginSessionRepository, ITokenGenerationService tokenGenerationService, IArgon2HashingService hashingService, ISHA256HashingService sha256HashingService, IDapperDb dapperDb)
    : IAuthenticationService
{
    public async Task CreateUserAsync(RegisterUserDto registerUserDto)
    {
        var isExistingUser = await userRepository.IsExistingUser(registerUserDto.Email);
        if (isExistingUser)
        {
            throw new Exception("This email is already taken. Please use another email address.");
        }
        
        var userDataModel = registerUserDto.MapTo<UserDataModel>();
        userDataModel.Password = await hashingService.Hash(registerUserDto.Password);
        userDataModel.SetAuditFieldsOnCreate();
        
        await userRepository.RegisterUser(userDataModel);
    }

    public async Task<TokenDetailsDto> LoginAsync(LoginDetailsDto loginDetailsDto, string userAgent, string? ipAddress)
    {
        var userDataModel = await userRepository.GetUserByEmail(loginDetailsDto.Email);
        if (userDataModel is null)
        {
            throw new Exception("User with this email doesn't exist.");
        }

        var isValidPassword = await hashingService.Verify(loginDetailsDto.Password, userDataModel.Password);
        if (!isValidPassword)
        {
            throw new UnauthorizedAccessException("Please provide valid credentials.");
        }
        
        var user = userDataModel.MapTo<User>();
        var currentDateTime = DateTime.UtcNow;
        var loginSession = new LoginSessionDataModel
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            DeviceId = loginDetailsDto.DeviceId,
            DeviceName = loginDetailsDto.DeviceName,
            UserAgent = userAgent,
            IpAddress = ipAddress,
            IsActive = true,
            LastSeenOn =  currentDateTime,
            DateCreated = currentDateTime,
        };
        
        var (jwtToken, refreshToken) = GenerateTokens(user, loginSession.Id);
        var existingRefreshToken = await refreshTokenRepository.GetRefreshToken(user.Id, loginSession.Id);
        try
        {
            dapperDb.BeginTransaction();
            if (existingRefreshToken is not null)
            {
                await refreshTokenRepository.RevokeRefreshToken(existingRefreshToken.RefreshToken);
            }
            
            await loginSessionRepository.CreateLoginSessionAsync(loginSession);
            await refreshTokenRepository.CreateRefreshToken(refreshToken);
            dapperDb.CommitTransaction();
        }
        catch (Exception)
        {
            dapperDb.RollbackTransaction();
            throw;
        }

        return new TokenDetailsDto(jwtToken, refreshToken.RefreshToken);
    }

    public async Task LogoutAsync(SessionDetailsDto sessionDetailsDto, string jwtToken, DateTime expiresOn)
    {
        var userDataModel = await userRepository.GetUserById(sessionDetailsDto.UserId);
        if (userDataModel is null)
        {
            throw new Exception("User doesn't exist.");
        }

        var loginSession = await loginSessionRepository.GetLoginSessionAsync(sessionDetailsDto.SessionId);
        if (loginSession is null)
        {
            throw new Exception("Login session doesn't exist.");
        }

        if (loginSession.UserId != userDataModel.Id)
        {
            throw new Exception("Login session doesn't belong to the user.");
        }

        if (!loginSession.IsActive)
        {
            throw new Exception("Login session is not active.");
        }

        var (refreshTokenDataModel, revokedToken) = await GetRevokedTokenAndRefreshTokenAsync(sessionDetailsDto.UserId, loginSession.Id, jwtToken, expiresOn);
        try
        {
            dapperDb.BeginTransaction();
            await loginSessionRepository.DeactivateLoginSessionAsync(loginSession.Id);
            await revokedTokenRepository.CreateRevokedToken(revokedToken);
            if (refreshTokenDataModel is not null)
            {
                await refreshTokenRepository.RevokeRefreshToken(refreshTokenDataModel.RefreshToken);
            }
            
            dapperDb.CommitTransaction();
        }
        catch (Exception)
        {
            dapperDb.RollbackTransaction();
            throw;
        }
    }

    public async Task<TokenDetailsDto> RefreshTokenAsync(SessionDetailsDto sessionDetailsDto, string jwtToken, DateTime expiresOn)
    {
        var userDataModel = await userRepository.GetUserById(sessionDetailsDto.UserId);
        if (userDataModel is null)
        {
            throw new Exception("User doesn't exist.");
        }

        var loginSession = await loginSessionRepository.GetLoginSessionAsync(sessionDetailsDto.SessionId);
        if (loginSession is null)
        {
            throw new Exception("Login session doesn't exist.");
        }

        if (loginSession.UserId != userDataModel.Id)
        {
            throw new Exception("Login session doesn't belong to the user.");
        }

        if (!loginSession.IsActive)
        {
            throw new Exception("Login session is not active.");
        }

        var user = userDataModel.MapTo<User>();
        var (newJwtToken, newRefreshTokenDataModel) = GenerateTokens(user, loginSession.Id);
        var (refreshTokenDataModel, revokedToken) = await GetRevokedTokenAndRefreshTokenAsync(sessionDetailsDto.UserId, loginSession.Id, jwtToken, expiresOn);
        try
        {
            dapperDb.BeginTransaction();
            await revokedTokenRepository.CreateRevokedToken(revokedToken);
            if (refreshTokenDataModel is not null)
            {
                await refreshTokenRepository.RevokeRefreshToken(refreshTokenDataModel.RefreshToken);
            }
            
            await refreshTokenRepository.CreateRefreshToken(newRefreshTokenDataModel);
            dapperDb.CommitTransaction();
        }
        catch (Exception ex)
        {
            dapperDb.RollbackTransaction();
            throw;
        }

        return new TokenDetailsDto(newJwtToken, newRefreshTokenDataModel.RefreshToken);
    }

    public async Task<bool> IsRevokedTokenAsync(string jwtToken)
    {
        var hashedToken = sha256HashingService.Hash(jwtToken);
        return await revokedTokenRepository.IsRevokedToken(hashedToken);
    }

    public async Task<bool> IsValidLoginSessionAsync(Guid userId, Guid sessionId)
    {
        return await loginSessionRepository.IsValidLoginSessionAsync(userId, sessionId);
    }

    #region Private Methods

    private (string JwtToken, RefreshTokenDataModel RefreshTokenDataModel) GenerateTokens(User user, Guid sessionId)
    {
        var jwtToken = tokenGenerationService.GenerateJwtToken(user, sessionId);
        var refreshToken = tokenGenerationService.GenerateRefreshToken(user.Id, sessionId);
        return (jwtToken, refreshToken);
    }

    private async Task<(RefreshTokenDataModel? RefreshTokenDataModel, RevokedTokenDataModel RevokedToken)> GetRevokedTokenAndRefreshTokenAsync(Guid userId, Guid sessionId, string jwtToken, DateTime expiresOn)
    {
        var refreshTokenDataModel = await refreshTokenRepository.GetRefreshToken(userId, sessionId);
        var hashedToken = sha256HashingService.Hash(jwtToken);
        var revokedToken = new RevokedTokenDataModel
        {
            HashedToken = hashedToken,
            ExpiresOn = expiresOn
        };
        
        revokedToken.SetAuditFieldsOnCreate();
        return (refreshTokenDataModel, revokedToken);
    }

    #endregion
}