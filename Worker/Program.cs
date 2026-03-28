using Azure.Identity;
using Extensions.Mapper;
using Worker;

var builder = Host.CreateApplicationBuilder(args);

var azureKeyVaultUriConfiguration = builder.Configuration.GetRequiredSection("AzureKeyVaultUri");
if (string.IsNullOrEmpty(azureKeyVaultUriConfiguration.Value))
{
    throw new InvalidOperationException("AzureKeyVaultUri is required.");
}

var azureKeyVaultUri = new Uri(azureKeyVaultUriConfiguration.Value);
builder.Configuration.AddAzureKeyVault(azureKeyVaultUri, new DefaultAzureCredential());

builder.Services.AddHostedService<Worker.Worker>();
builder.Services.RegisterWorkerService();

var host = builder.Build();

MapperFactory.InitializeServiceProvider(host.Services);

host.Run();