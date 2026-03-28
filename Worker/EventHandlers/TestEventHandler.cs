using Models.Events;

namespace Worker.EventHandlers;

public class TestEventHandler : IEventHandler<TestEvent>
{
    public async Task HandleAsync(TestEvent worker)
    {
        Console.WriteLine($"Test Event Triggered:\n{worker.UserIdentifier}: {worker.Message}");
        await Task.CompletedTask;
    }
}