using Models.DataModels;

namespace Services.Repositories.RevokedToken;

public interface IRevokedTokenRepository
{
    Task<bool> IsRevokedToken(string hashedToken);
    
    Task CreateRevokedToken(RevokedTokenDataModel revokedToken);

    Task DeleteRevokedTokenByIdAsync(Guid id);
}