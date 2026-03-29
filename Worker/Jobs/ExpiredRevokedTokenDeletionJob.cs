using Microsoft.Extensions.Logging;
using Services.Repositories.RevokedToken;

namespace Worker.Jobs;

public class ExpiredRevokedTokenDeletionJob(
    IRevokedTokenRepository revokedTokenRepository,
    ILogger<ExpiredRevokedTokenDeletionJob> logger)
{
    public async Task ExecuteAsync(Guid revokedTokenId)
    {
        logger.LogInformation(
            "ExpiredRevokedTokenDeletion job started for RevokedTokenId={RevokedTokenId}",
            revokedTokenId);

        await revokedTokenRepository.DeleteRevokedTokenByIdAsync(revokedTokenId);

        logger.LogInformation(
            "ExpiredRevokedTokenDeletion job completed for RevokedTokenId={RevokedTokenId}",
            revokedTokenId);
    }
}
