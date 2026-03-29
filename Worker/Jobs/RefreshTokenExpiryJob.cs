using Microsoft.Extensions.Logging;
using Services.Repositories.LoginSession;
using Services.Repositories.RefreshToken;

namespace Worker.Jobs;

public class RefreshTokenExpiryJob(
    IRefreshTokenRepository refreshTokenRepository,
    ILoginSessionRepository loginSessionRepository,
    ILogger<RefreshTokenExpiryJob> logger)
{
    public async Task ExecuteAsync(Guid refreshTokenId, Guid sessionId)
    {
        logger.LogInformation(
            "RefreshTokenExpiry job started for RefreshTokenId={RefreshTokenId}, SessionId={SessionId}",
            refreshTokenId, sessionId);

        await refreshTokenRepository.RevokeRefreshTokenByIdAsync(refreshTokenId);
        await loginSessionRepository.DeactivateLoginSessionAsync(sessionId);

        logger.LogInformation(
            "RefreshTokenExpiry job completed for RefreshTokenId={RefreshTokenId}, SessionId={SessionId}",
            refreshTokenId, sessionId);
    }
}
