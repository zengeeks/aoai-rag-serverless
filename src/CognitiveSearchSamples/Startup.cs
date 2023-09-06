using Azure;
using Azure.AI.OpenAI;
using Azure.Core.Serialization;
using Azure.Identity;
using Azure.Search.Documents;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text.Json;

[assembly: FunctionsStartup(typeof(CognitiveSearchSamples.Startup))]

namespace CognitiveSearchSamples;

internal class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        // AddAzureClients / AddOpenAIClient / AddSearchClient 拡張メソッドを使った DI 方法もあるが今回はプリミティブに実装

        builder.Services.AddSingleton(_ =>
        {
            var endpoint = new Uri(Environment.GetEnvironmentVariable("AzureOpenAIOptions:Endpoint") ?? throw new NullReferenceException("AzureOpenAIOptions:Endpoint"));
            return new OpenAIClient(endpoint, new DefaultAzureCredential());  // Managed Identity で認証
        });

        builder.Services.AddSingleton(_ =>
        {
            var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            // C# の class (pascal-case) と CognitiveSearch index schema (camel-case) を補完するためのオプション
            var searchClientOptions = new SearchClientOptions { Serializer = new JsonObjectSerializer(jsonSerializerOptions) };

            var indexName = Environment.GetEnvironmentVariable("CognitiveSearchOptions:IndexName") ?? throw new NullReferenceException("CognitiveSearchOptions:IndexName");
            var endpoint = new Uri(Environment.GetEnvironmentVariable("CognitiveSearchOptions:Endpoint") ?? throw new NullReferenceException("CognitiveSearchOptions:IndexName"));

            // Key で認証
            return new SearchClient(endpoint, indexName, new AzureKeyCredential(Environment.GetEnvironmentVariable("CognitiveSearchOptions:AdminKey")), searchClientOptions);
            // Managed Identity で認証
            //return new SearchClient(endpoint, indexName, new DefaultAzureCredential(), searchClientOptions); // Managed Identity で認証
        });
    }
}