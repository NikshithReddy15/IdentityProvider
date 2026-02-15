using DatabaseAdapter.Dapper;
using Encryption.Hashing.Argon2Hashing;
using Encryption.Hashing.SHA256Hashing;
using Extensions;
using Extensions.Mapper;
using Models.CoreModels;
using Models.DataModels;
using Models.DataTransferObjects;
using Services.Repositories.RefreshToken;
using Services.Repositories.RevokedToken;
using Services.Repositories.User;
using Services.TokenGeneration;

namespace Services.Authentication;

public class AuthenticationService(IUserRepository userRepository, IRevokedTokenRepository revokedTokenRepository, IRefreshTokenRepository refreshTokenRepository, ITokenGenerationService tokenGenerationService, IArgon2HashingService hashingService, ISHA256HashingService sha256HashingService, IDapperDb dapperDb)
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

    public async Task<TokenDetailsDto> LoginAsync(LoginDetailsDto loginDetailsDto)
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
        var (jwtToken, refreshToken) = GenerateTokens(user);
        var existingRefreshToken = await refreshTokenRepository.GetRefreshToken(user.Id);
        try
        {
            dapperDb.BeginTransaction();
            if (existingRefreshToken is not null)
            {
                await refreshTokenRepository.RevokeRefreshToken(existingRefreshToken.RefreshToken);
            }
            
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

    public async Task LogoutAsync(Guid userId, string jwtToken, DateTime expiresOn)
    {
        var (refreshTokenDataModel, revokedToken) = await GetRevokedTokenAndRefreshTokenAsync(userId, jwtToken, expiresOn);
        try
        {
            dapperDb.BeginTransaction();
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

    public async Task<TokenDetailsDto> RefreshTokenAsync(Guid userId, string jwtToken, DateTime expiresOn)
    {
        var userDataModel = await userRepository.GetUserById(userId);
        if (userDataModel is null)
        {
            throw new Exception("User details doesn't exist.");
        }
        
        var user = userDataModel.MapTo<User>();
        var (newJwtToken, newRefreshTokenDataModel) = GenerateTokens(user);
        var (refreshTokenDataModel, revokedToken) = await GetRevokedTokenAndRefreshTokenAsync(userId, jwtToken, expiresOn);
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

    #region Private Methods

    private (string JwtToken, RefreshTokenDataModel RefreshTokenDataModel) GenerateTokens(User user)
    {
        var jwtToken = tokenGenerationService.GenerateJwtToken(user);
        var refreshToken = tokenGenerationService.GenerateRefreshToken(user.Id);
        return (jwtToken, refreshToken);
    }

    private async Task<(RefreshTokenDataModel? RefreshTokenDataModel, RevokedTokenDataModel RevokedToken)> GetRevokedTokenAndRefreshTokenAsync(Guid userId, string jwtToken, DateTime expiresOn)
    {
        var refreshTokenDataModel = await refreshTokenRepository.GetRefreshToken(userId);
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