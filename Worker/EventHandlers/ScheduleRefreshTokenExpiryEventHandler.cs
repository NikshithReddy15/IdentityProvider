using Hangfire;
using Models.Events;
using Worker.Jobs;

namespace Worker.EventHandlers;

internal class ScheduleRefreshTokenExpiryEventHandler(IBackgroundJobClient backgroundJobClient) : IEventHandler<ScheduleRefreshTokenExpiryEvent>
{
    public Task HandleAsync(ScheduleRefreshTokenExpiryEvent @event)
    {
        backgroundJobClient.Schedule<RefreshTokenExpiryJob>(
            job => job.ExecuteAsync(@event.RefreshTokenId, @event.SessionId),
            @event.ExpiresAt);
        return Task.CompletedTask;
    }
}
