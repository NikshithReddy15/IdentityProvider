using Models.Events;
using Worker.Email;
using Worker.EventDispatcher;
using Worker.EventHandlers;

namespace Worker;

public static class ServiceCollectionExtensions
{
    public static void RegisterWorkerService(this IServiceCollection services)
    {
        RegisterEventDispatcher(services);
        RegisterServices(services);
    }

    private static void RegisterEventDispatcher(IServiceCollection services)
    {
        services.AddSingleton<IEventDispatcher, EventDispatcher.EventDispatcher>();
    }

    private static void RegisterServices(IServiceCollection services)
    {
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IEventHandler<TestEvent>, TestEventHandler>();
        services.AddScoped<IEventHandler<RegistrationOtpEvent>, RegistrationOtpEventHandler>();
    }
}