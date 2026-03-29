using DatabaseAdapter.Dapper;
using Models.Events;
using Services;
using Worker.Email;
using Worker.EventDispatcher;
using Worker.EventHandlers;
using Worker.Jobs;

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
        services.AddScoped<IEventHandler<RegistrationOtpEvent>, RegistrationOtpEventHandler>();
        services.AddScoped<IEventHandler<RefreshTokenExpiryEvent>, RefreshTokenExpiryEventHandler>();
        services.AddScoped<IEventHandler<RevokedTokenCleanupEvent>, RevokedTokenCleanupEventHandler>();
        services.AddSingleton<IDapperDb, DapperDb>();
        services.RegisterWorkerDependencies();
        services.AddScoped<OtpCleanupJob>();
        services.AddScoped<TokenExpiryJob>();
    }
}