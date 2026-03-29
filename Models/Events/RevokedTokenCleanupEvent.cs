namespace Models.Events;

public class RevokedTokenCleanupEvent : BaseEvent
{
    public Guid RevokedTokenId { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}
