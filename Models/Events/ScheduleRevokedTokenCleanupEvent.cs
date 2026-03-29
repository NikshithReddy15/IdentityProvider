namespace Models.Events;

public class ScheduleRevokedTokenCleanupEvent : BaseEvent
{
    public Guid RevokedTokenId { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}
