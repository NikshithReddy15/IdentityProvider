using Azure.Messaging.ServiceBus;
using Extensions;
using Microsoft.Extensions.Configuration;

namespace Services.Events;

internal class QueueMessageSender : IQueueMessageSender
{
    private readonly ServiceBusSender _sender;
    
    public QueueMessageSender(IConfiguration configuration)
    {
        var serviceBusSection = configuration.GetRequiredSection("ServiceBus");
        var client = new ServiceBusClient(serviceBusSection["ServiceBusConnectionString"]);
        _sender = client.CreateSender(serviceBusSection["QueueName"]);
    }
    
    public async Task SendMessageAsync<T>(T message, DateTime? scheduledEnqueueTimeUtc = null)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        var serviceBusMessage = new ServiceBusMessage(message.ToJson())
        {
            Subject = typeof(T).AssemblyQualifiedName
        };
        if (scheduledEnqueueTimeUtc.HasValue)
        {
            if (scheduledEnqueueTimeUtc.Value < DateTime.UtcNow)
            {
                throw new Exception($"The scheduled enqueue time must be in the future for {nameof(message)}.");
            }
            
            serviceBusMessage.ScheduledEnqueueTime = scheduledEnqueueTimeUtc.Value;
        }

        await _sender.SendMessageAsync(serviceBusMessage);
    }
}