using DatabaseAdapter.Dapper;
using Encryption.Hashing;
using Encryption.Hashing.Argon2Hashing;
using Encryption.Hashing.SHA256Hashing;
using Extensions.Mapper;
using Microsoft.Extensions.DependencyInjection;
using Services.Authentication;
using Services.Repositories.RevokedToken;
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
        services.AddScoped<ITokenGenerationService, TokenGenerationService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        RegisterRepositories(services);
        RegisterEncryptionServices(services);
    }

    private static void RegisterRepositories(IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRevokedTokenRepository, RevokedTokenRepository>();
    }

    private static void RegisterEncryptionServices(IServiceCollection services)
    {
        services.AddScoped<IArgon2HashingService, Argon2HashingService>();
        services.AddScoped<ISHA256HashingService, SHA256HashingService>();
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