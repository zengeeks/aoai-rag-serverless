using Azure.AI.OpenAI;
using Azure.Search.Documents;
using CognitiveSearchSamples.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CognitiveSearchSamples;

public class IndexChangeFeed
{
    private const string EmbeddingsDeploymentName = "text-embedding-ada-002";

    private readonly OpenAIClient _openAIClient;
    private readonly SearchClient _searchClient;

    public IndexChangeFeed(OpenAIClient openAIClient, SearchClient searchClient)
    {
        _openAIClient = openAIClient;
        _searchClient = searchClient;
    }

    [FunctionName("upload")]
    public async Task UploadIndexDocumentAsync([CosmosDBTrigger(
            databaseName: "Aoai",
            containerName: "AzureInfo",
            Connection = "CosmosConnection",
            FeedPollDelay = 1000,
            LeaseContainerName = "leases")]IReadOnlyList<AzureInfo> input,
        ILogger log)
    {
        var documentsToUpload = new List<AzureIndexDocument>();

        foreach (var azureInfo in input)
        {
            var contentVector = await GenerateEmbeddingsAsync(azureInfo.Content);

            documentsToUpload.Add(new AzureIndexDocument
            {
                Id = azureInfo.Id,
                Category = azureInfo.Category,
                Title = azureInfo.Title,
                Content = azureInfo.Content,
                ContentVector = contentVector
            });
        }

        await _searchClient.MergeOrUploadDocumentsAsync(documentsToUpload);
        log.LogInformation($"{documentsToUpload.Count} document(s) uploaded.");
    }

    private async Task<IReadOnlyList<float>> GenerateEmbeddingsAsync(string text)
    {
        var response = await _openAIClient.GetEmbeddingsAsync(EmbeddingsDeploymentName, new EmbeddingsOptions(text));
        return response.Value.Data[0].Embedding;
    }
}