using DatabaseAdapter.Dapper;
using Encryption.Hashing;
using Extensions.Mapper;
using Microsoft.Extensions.DependencyInjection;
using Services.Authentication;
using Services.Repositories.User;
using Services.TokenGeneration;

namespace Services;

public static class ServiceCollectionExtensions
{
    public static void RegisterAuthenticationWebApiServices(this IServiceCollection services)
    {
        RegisterServices(services);
        RegisterMappingProfiles(services);
        RegisterDatabaseAdapters(services);
    }

    #region Services

    private static void RegisterServices(IServiceCollection services)
    {
        RegisterEncryptionServices(services);
        services.AddScoped<ITokenGenerationService, TokenGenerationService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IUserRepository, UserRepository>();
    }

    private static void RegisterEncryptionServices(IServiceCollection services)
    {
        services.AddScoped<IArgon2HashingService, Argon2HashingService>();
    }

    #endregion

    #region Mapping Profiles

    private static void RegisterMappingProfiles(IServiceCollection services)
    {
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile(new MappingProfile());
        });
    }

    #endregion

    #region Database Adapters

    private static void RegisterDatabaseAdapters(IServiceCollection services)
    {
        services.AddSingleton<IDapperDb, DapperDb>();
    }

    #endregion
}