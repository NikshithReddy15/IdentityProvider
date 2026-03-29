using Hangfire;
using Models.Events;
using Worker.Jobs;

namespace Worker.EventHandlers;

internal class ScheduleRevokedTokenCleanupEventHandler(IBackgroundJobClient backgroundJobClient) : IEventHandler<ScheduleRevokedTokenCleanupEvent>
{
    public Task HandleAsync(ScheduleRevokedTokenCleanupEvent @event)
    {
        backgroundJobClient.Schedule<ExpiredRevokedTokenDeletionJob>(
            job => job.ExecuteAsync(@event.RevokedTokenId),
            @event.ExpiresAt);
        return Task.CompletedTask;
    }
}
