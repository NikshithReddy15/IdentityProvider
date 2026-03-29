using Models.Events;
using Services.Repositories.RevokedToken;

namespace Worker.EventHandlers;

internal class RevokedTokenCleanupEventHandler(IRevokedTokenRepository revokedTokenRepository) : IEventHandler<RevokedTokenCleanupEvent>
{
    public async Task HandleAsync(RevokedTokenCleanupEvent @event)
    {
        var delay = @event.ExpiresAt - DateTimeOffset.UtcNow;
        if (delay < TimeSpan.Zero)
        {
            delay = TimeSpan.Zero;
        }

        await Task.Delay(delay);
        await revokedTokenRepository.DeleteRevokedTokenByIdAsync(@event.RevokedTokenId);
    }
}
