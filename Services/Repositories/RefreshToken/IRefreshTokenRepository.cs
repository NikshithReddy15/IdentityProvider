using Models.DataModels;

namespace Services.Repositories.RefreshToken;

public interface IRefreshTokenRepository
{
    Task CreateRefreshToken(RefreshTokenDataModel refreshToken);
    
    Task<RefreshTokenDataModel?> GetRefreshToken(Guid userId, Guid sessionId);
    
    Task RevokeRefreshToken(string refreshToken);

    Task RevokeRefreshTokenByIdAsync(Guid id);

    Task<IEnumerable<Guid>> RevokeExpiredRefreshTokensAsync();
}