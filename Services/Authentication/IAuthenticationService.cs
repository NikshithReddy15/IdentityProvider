using Models.DataTransferObjects;

namespace Services.Authentication;

public interface IAuthenticationService
{
    Task CreateUserAsync(RegisterUserDto registerUserDto);
    
    Task<TokenDetailsDto> LoginAsync(LoginDetailsDto loginDetailsDto);

    Task LogoutAsync(Guid userId, string jwtToken, DateTime expiresOn);

    Task<TokenDetailsDto> RefreshTokenAsync(Guid userId, string jwtToken, DateTime expiresOn);

    Task<bool> IsRevokedTokenAsync(string jwtToken);
}