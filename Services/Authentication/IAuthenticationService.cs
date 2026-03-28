using Models.DataTransferObjects;

namespace Services.Authentication;

public interface IAuthenticationService
{
    Task CreateUserAsync(RegisterUserDto registerUserDto);

    Task VerifyRegistrationOtpAsync(VerifyOtpDto verifyOtpDto);

    Task<TokenDetailsDto> LoginAsync(EmailLoginDetailsDto emailLoginDetailsDto, string userAgent, string? ipAddress);

    Task LogoutAsync(SessionDetailsDto sessionDetailsDto, string jwtToken, DateTime expiresOn);

    Task<TokenDetailsDto> RefreshTokenAsync(SessionDetailsDto sessionDetailsDto, string jwtToken, DateTime expiresOn);

    Task<bool> IsRevokedTokenAsync(string jwtToken);

    Task<bool> IsValidLoginSessionAsync(Guid userId, Guid sessionId);
}