namespace Models.Events;

public class ScheduleRefreshTokenExpiryEvent : BaseEvent
{
    public Guid RefreshTokenId { get; set; }
    public Guid SessionId { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}
