namespace Worker.EventDispatcher;

public interface IEventDispatcher
{
    Task HandleAsync(string eventType, string message);
}