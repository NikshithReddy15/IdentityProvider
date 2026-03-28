using DatabaseAdapter.Dapper;
using Encryption.Hashing.Argon2Hashing;
using Encryption.Hashing.SHA256Hashing;
using Extensions;
using Extensions.Mapper;
using Models.CoreModels;
using Models.DataModels;
using Models.DataTransferObjects;
using Models.Enums;
using Models.Events;
using Services.Events;
using Services.Repositories.LoginSession;
using Services.Repositories.RefreshToken;
using Services.Repositories.RevokedToken;
using Services.Repositories.User;
using Services.Repositories.UserOtp;
using Services.TokenGeneration;

namespace Services.Authentication;

public class AuthenticationService(IUserRepository userRepository, IUserOtpRepository userOtpRepository, IRevokedTokenRepository revokedTokenRepository, IRefreshTokenRepository refreshTokenRepository, ILoginSessionRepository loginSessionRepository, ITokenGenerationService tokenGenerationService, IArgon2HashingService hashingService, ISHA256HashingService sha256HashingService, IDapperDb dapperDb, IQueueMessageSender queueMessageSender)
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
        userDataModel.Id = Guid.NewGuid();
        userDataModel.Password = await hashingService.Hash(registerUserDto.Password);
        userDataModel.AccountStatus = (sbyte)AccountStatus.NotVerified;
        userDataModel.SetAuditFieldsOnCreate();

        var otpCode = GenerateOtpCode();
        var userOtp = new UserOtpDataModel
        {
            Id = Guid.NewGuid(),
            UserId = userDataModel.Id,
            OtpCode = otpCode,
            ExpiresOn = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false,
            DateCreated = DateTime.UtcNow,
        };

        try
        {
            dapperDb.BeginTransaction();
            await userRepository.RegisterUser(userDataModel);
            await userOtpRepository.CreateOtpAsync(userOtp);
            dapperDb.CommitTransaction();
        }
        catch (Exception)
        {
            dapperDb.RollbackTransaction();
            throw;
        }

        await queueMessageSender.SendMessageAsync(new RegistrationOtpEvent
        {
            UserIdentifier = userDataModel.Id,
            Email = userDataModel.Email,
            FirstName = userDataModel.FirstName,
            OtpCode = otpCode,
        });
    }

    public async Task VerifyRegistrationOtpAsync(VerifyOtpDto verifyOtpDto)
    {
        var userDataModel = await userRepository.GetUserByEmail(verifyOtpDto.Email);
        if (userDataModel is null)
        {
            throw new Exception("User with this email doesn't exist.");
        }

        if (userDataModel.AccountStatus != (sbyte)AccountStatus.NotVerified)
        {
            throw new Exception("Account is already verified.");
        }

        var userOtp = await userOtpRepository.GetActiveOtpAsync(userDataModel.Id);
        if (userOtp is null)
        {
            throw new Exception("No valid OTP found. Please request a new one.");
        }

        if (userOtp.OtpCode != verifyOtpDto.OtpCode)
        {
            throw new UnauthorizedAccessException("Invalid OTP.");
        }

        try
        {
            dapperDb.BeginTransaction();
            await userOtpRepository.MarkOtpAsUsedAsync(userOtp.Id);
            await userRepository.UpdateAccountStatusAsync(userDataModel.Id, (sbyte)AccountStatus.Active);
            dapperDb.CommitTransaction();
        }
        catch (Exception)
        {
            dapperDb.RollbackTransaction();
            throw;
        }
    }

    public async Task<TokenDetailsDto> LoginAsync(EmailLoginDetailsDto emailLoginDetailsDto, string userAgent, string? ipAddress)
    {
        var userDataModel = await userRepository.GetUserByEmail(emailLoginDetailsDto.Email);
        if (userDataModel is null)
        {
            throw new Exception("User with this email doesn't exist.");
        }

        var isValidPassword = await hashingService.Verify(emailLoginDetailsDto.Password, userDataModel.Password);
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
            DeviceId = emailLoginDetailsDto.DeviceId,
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

    private static string GenerateOtpCode()
    {
        return Random.Shared.Next(100000, 999999).ToString();
    }

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