namespace Models.Events;

public class TestEvent : BaseEvent
{
    public required string Message { get; set; }
}