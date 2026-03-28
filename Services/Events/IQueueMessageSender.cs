namespace Services.Events;

public interface IQueueMessageSender
{
    Task SendMessageAsync<T>(T message, DateTime? scheduledEnqueueTimeUtc = null);
}