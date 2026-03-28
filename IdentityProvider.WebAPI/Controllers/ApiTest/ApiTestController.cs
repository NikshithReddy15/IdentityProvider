using Microsoft.AspNetCore.Mvc;
using Models.Events;
using Services.Events;

namespace IdentityProvider.WebAPI.Controllers.ApiTest;

[ApiController]
[Route("api/api-test")]
public class ApiTestController(IQueueMessageSender queueMessageSender) : ControllerBase
{
    [HttpGet("test-service-bus")]
    public async Task TestServiceBus()
    {
        var testEvent = new TestEvent
        {
            UserIdentifier = Guid.NewGuid(),
            Message = "This is a test message",
        };
        
        await queueMessageSender.SendMessageAsync(testEvent);
    }

    [HttpGet("test-service-bus-delayed")]
    public async Task TestServiceBusDelayed()
    {
        var testEvent = new TestEvent
        {
            UserIdentifier = Guid.NewGuid(),
            Message = "This is a test message with 10 seconds delay",
        };

        await queueMessageSender.SendMessageAsync(testEvent, DateTime.UtcNow.AddSeconds(10));
    }
}