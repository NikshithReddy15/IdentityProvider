namespace Worker.EventHandlers;

public interface IEventHandler<in TEvent>
{
    Task HandleAsync(TEvent worker);
}