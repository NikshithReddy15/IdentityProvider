using Models.DataModels;

namespace Services.Repositories.RefreshToken;

public interface IRefreshTokenRepository
{
    Task CreateRefreshToken(RefreshTokenDataModel refreshToken);
    
    Task<RefreshTokenDataModel?> GetRefreshToken(Guid userId);
    
    Task RevokeRefreshToken(string refreshToken);
}