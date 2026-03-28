using Azure.Messaging.ServiceBus;
using Worker.EventDispatcher;

namespace Worker;

public class Worker : BackgroundService
{
    private readonly ServiceBusProcessor _serviceBusProcessor;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    
    public Worker(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
    {
        var serviceBusSection = configuration.GetRequiredSection("ServiceBus");
        var client = new ServiceBusClient(serviceBusSection["ServiceBusConnectionString"]);
        _serviceBusProcessor = client.CreateProcessor(serviceBusSection["QueueName"]);
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _serviceBusProcessor.ProcessMessageAsync += ProcessMessagesAsync;
        _serviceBusProcessor.ProcessErrorAsync += ProcessErrorsAsync;
        
        await _serviceBusProcessor.StartProcessingAsync(cancellationToken);
    }

    private async Task ProcessMessagesAsync(ProcessMessageEventArgs args)
    {
        var message = args.Message.Body.ToString();
        Console.WriteLine($"Received message: {message}");
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var dispatcher = scope.ServiceProvider.GetRequiredService<IEventDispatcher>();
            await dispatcher.HandleAsync(args.Message.Subject, message);
            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            await args.AbandonMessageAsync(args.Message);
            throw new Exception($"An error occured while processing event.", ex);
        }
    }

    private async Task ProcessErrorsAsync(ProcessErrorEventArgs args)
    {
        Console.WriteLine($"An error occured while processing event:\n{args.Exception}");
        await Task.CompletedTask;
    }
    
    private async Task StopProcessingAsync(CancellationToken cancellationToken)
    {
        await _serviceBusProcessor.StopProcessingAsync(cancellationToken);
        await _serviceBusProcessor.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }
}