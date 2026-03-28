using System.Text.Json;
using Worker.EventHandlers;

namespace Worker.EventDispatcher;

public class EventDispatcher (IServiceProvider serviceProvider) : IEventDispatcher
{
    public async Task HandleAsync(string eventType, string message)
    {
        if (string.IsNullOrWhiteSpace(eventType))
        {
            throw new ArgumentException("Message Subject is missing. The sender must set Subject to the event type's AssemblyQualifiedName (see QueueMessageSender).", nameof(eventType));
        }

        var type = Type.GetType(eventType);
        if (type is null)
        {
            throw new ArgumentException($"Could not resolve event type '{eventType}'. Use the full assembly-qualified type name (typeof(T).AssemblyQualifiedName).", nameof(eventType));
        }

        var @event = JsonSerializer.Deserialize(message, type);
        if (@event is null)
        {
            throw new ArgumentException("Message body could not be deserialized to the event type.", nameof(message));
        }

        var handlerType = typeof(IEventHandler<>).MakeGenericType(type);
        using (var scope = serviceProvider.CreateScope())
        {
            var service = scope.ServiceProvider.GetRequiredService(handlerType);
            var method = handlerType.GetMethod("HandleAsync");
            if (method is null)
            {
                throw new InvalidOperationException($"HandleAsync not found on {handlerType}.");
            }

            await (Task)method.Invoke(service, [@event])!;
        }
    }
}