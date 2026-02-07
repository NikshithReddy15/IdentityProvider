using Encryption.Hashing;
using Extensions;
using Extensions.Mapper;
using Models.CoreModels;
using Models.DataModels;
using Models.DataTransferObjects;
using Services.Repositories.User;
using Services.TokenGeneration;

namespace Services.Authentication;

public class AuthenticationService(IUserRepository userRepository, ITokenGenerationService tokenGenerationService, IArgon2HashingService hashingService)
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
}