using DatabaseAdapter.Dapper;
using Models.Events;
using Services.Repositories.LoginSession;
using Services.Repositories.RefreshToken;

namespace Worker.EventHandlers;

internal class RefreshTokenExpiryEventHandler(IRefreshTokenRepository refreshTokenRepository, ILoginSessionRepository loginSessionRepository, IDapperDb dapperDb) : IEventHandler<RefreshTokenExpiryEvent>
{
    public async Task HandleAsync(RefreshTokenExpiryEvent @event)
    {
        var delay = @event.ExpiresAt - DateTimeOffset.UtcNow;
        if (delay < TimeSpan.Zero)
        {
            delay = TimeSpan.Zero;
        }

        await Task.Delay(delay);
        try
        {
            dapperDb.BeginTransaction();
            await refreshTokenRepository.RevokeRefreshTokenByIdAsync(@event.RefreshTokenId);
            await loginSessionRepository.DeactivateLoginSessionAsync(@event.SessionId);
            dapperDb.CommitTransaction();
        }
        catch
        {
            dapperDb.RollbackTransaction();
            throw;
        }
    }
}
