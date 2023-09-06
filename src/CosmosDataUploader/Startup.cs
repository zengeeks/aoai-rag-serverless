using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using Azure.Identity;
using CosmosDataUploader;

[assembly: FunctionsStartup(typeof(Startup))]

namespace CosmosDataUploader;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddSingleton(_ =>
        {
            var options = new CosmosClientOptions()
            {
                SerializerOptions = new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase }
            };
            // Managed Identity で認証
            return new CosmosClient(Environment.GetEnvironmentVariable("CosmosEndpoint"), new DefaultAzureCredential(), options);
        });
    }
}