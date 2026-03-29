using Azure.Identity;
using Extensions.Mapper;
using Hangfire;
using Hangfire.SqlServer;
using Worker;
using Worker.Jobs;

var builder = Host.CreateApplicationBuilder(args);

var azureKeyVaultUriConfiguration = builder.Configuration.GetRequiredSection("AzureKeyVaultUri");
if (string.IsNullOrEmpty(azureKeyVaultUriConfiguration.Value))
{
    throw new InvalidOperationException("AzureKeyVaultUri is required.");
}

var azureKeyVaultUri = new Uri(azureKeyVaultUriConfiguration.Value);
builder.Configuration.AddAzureKeyVault(azureKeyVaultUri, new DefaultAzureCredential());

var connectionString = builder.Configuration.GetConnectionString("IdentityDatabaseConnectionString")
    ?? throw new InvalidOperationException("IdentityDatabaseConnectionString is required.");

builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true,
    }));

builder.Services.AddHangfireServer();
builder.Services.AddHostedService<Worker.Worker>();
builder.Services.RegisterWorkerService();

var host = builder.Build();

MapperFactory.InitializeServiceProvider(host.Services);

var recurringJobManager = host.Services.GetRequiredService<IRecurringJobManager>();
recurringJobManager.AddOrUpdate<OtpCleanupJob>(
    "otp-cleanup",
    job => job.ExecuteAsync(),
    "*/10 * * * *");

recurringJobManager.AddOrUpdate<TokenExpiryJob>(
    "token-expiry",
    job => job.ExecuteAsync(),
    "0 * * * *");

host.Run();