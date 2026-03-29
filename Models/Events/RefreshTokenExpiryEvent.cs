namespace Models.Events;

public class RefreshTokenExpiryEvent : BaseEvent
{
    public Guid RefreshTokenId { get; set; }
    public Guid SessionId { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}
