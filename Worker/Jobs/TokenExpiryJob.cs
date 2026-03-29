using DatabaseAdapter.Dapper;
using Services.Repositories.LoginSession;
using Services.Repositories.RefreshToken;
using Services.Repositories.RevokedToken;

namespace Worker.Jobs;

public class TokenExpiryJob(
    IRefreshTokenRepository refreshTokenRepository,
    ILoginSessionRepository loginSessionRepository,
    IRevokedTokenRepository revokedTokenRepository,
    ILogger<TokenExpiryJob> logger,
    IDapperDb dapperDb)
{
    public async Task ExecuteAsync()
    {
        logger.LogInformation($"TokenExpiry job started at {DateTime.UtcNow}");
        try
        {
            dapperDb.BeginTransaction();
            var expiredSessionIds = await refreshTokenRepository.RevokeExpiredRefreshTokensAsync();
            foreach (var sessionId in expiredSessionIds)
            {
                await loginSessionRepository.DeactivateLoginSessionAsync(sessionId);
            }

            await revokedTokenRepository.DeleteExpiredRevokedTokensAsync();
            dapperDb.CommitTransaction();
        }
        catch (Exception ex)
        {
            dapperDb.RollbackTransaction();
            logger.LogError(ex, "Error occurred while executing TokenExpiry job");
        }

        logger.LogInformation($"TokenExpiry job completed at {DateTime.UtcNow}");
    }
}
