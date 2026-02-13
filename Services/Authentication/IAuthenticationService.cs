using Models.DataTransferObjects;

namespace Services.Authentication;

public interface IAuthenticationService
{
    Task CreateUserAsync(RegisterUserDto registerUserDto);
    
    Task<string> LoginAsync(LoginDetailsDto loginDetailsDto);

    Task LogoutAsync(string jwtToken, DateTime expiresOn);

    Task<bool> IsRevokedTokenAsync(string jwtToken);
}