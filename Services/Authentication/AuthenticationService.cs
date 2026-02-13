using Encryption.Hashing;
using Encryption.Hashing.Argon2Hashing;
using Encryption.Hashing.SHA256Hashing;
using Extensions;
using Extensions.Mapper;
using Models.CoreModels;
using Models.DataModels;
using Models.DataTransferObjects;
using Services.Repositories.RevokedToken;
using Services.Repositories.User;
using Services.TokenGeneration;

namespace Services.Authentication;

public class AuthenticationService(IUserRepository userRepository, IRevokedTokenRepository revokedTokenRepository, ITokenGenerationService tokenGenerationService, IArgon2HashingService hashingService, ISHA256HashingService sha256HashingService)
    : IAuthenticationService
{
    public async Task CreateUserAsync(RegisterUserDto registerUserDto)
    {
        var isExistingUser = await userRepository.IsExistingUser(registerUserDto.Email);
        if (isExistingUser)
        {
            throw new Exception("User with that email already exists");
        }
        
        var userDataModel = registerUserDto.MapTo<UserDataModel>();
        userDataModel.Password = await hashingService.Hash(registerUserDto.Password);
        userDataModel.SetAuditFieldsOnCreate();
        
        await userRepository.RegisterUser(userDataModel);
    }

    public async Task<string> LoginAsync(LoginDetailsDto loginDetailsDto)
    {
        var user = await userRepository.GetUserByEmail(loginDetailsDto.Email);
        if (user is null)
        {
            throw new Exception("User with that email doesn't exist");
        }

        var isValidPassword = await hashingService.Verify(loginDetailsDto.Password, user.Password);
        if (!isValidPassword)
        {
            throw new UnauthorizedAccessException("Password is incorrect");
        }
        
        return tokenGenerationService.GenerateJwtToken(user.MapTo<User>());
    }

    public async Task LogoutAsync(string jwtToken, DateTime expiresOn)
    {
        var hashedToken = sha256HashingService.Hash(jwtToken);
        var revokedToken = new RevokedTokenDataModel
        {
            Id = Guid.NewGuid(),
            HashedToken = hashedToken,
            GeneratedOn =  DateTime.UtcNow,
            ExpiresOn = expiresOn
        };
        
        await revokedTokenRepository.CreateRevokedToken(revokedToken);
    }

    public async Task<bool> IsRevokedTokenAsync(string jwtToken)
    {
        var hashedToken = sha256HashingService.Hash(jwtToken);
        return await revokedTokenRepository.IsRevokedToken(hashedToken);
    }
}