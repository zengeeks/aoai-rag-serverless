using Azure.AI.OpenAI;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using CognitiveSearchSamples.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CognitiveSearchSamples;

public class VectorSearchApi
{
    private const string EmbeddingsDeploymentName = "text-embedding-ada-002";
    private const string VectorField = "contentVector";
    private const int DataCount = 3;

    private readonly OpenAIClient _openAIClient;
    private readonly SearchClient _searchClient;

    public VectorSearchApi(OpenAIClient openAIClient, SearchClient searchClient)
    {
        _openAIClient = openAIClient;
        _searchClient = searchClient;
    }

    [FunctionName("vector-search")]
    public async Task<IActionResult> SearchWithVector(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
        ILogger log)
    {
        var query = req.Query["query"];

        if (string.IsNullOrWhiteSpace(query))
        {
            return new BadRequestResult();
        }

        var queryEmbeddings = await GenerateEmbeddingsAsync(query);
        var searchOptions = new SearchOptions
        {
            Vectors = { new SearchQueryVector { Value = queryEmbeddings.ToArray(), KNearestNeighborsCount = 3, Fields = { VectorField } } },
            Size = DataCount,
            Select = { "title", "content", "category" }
        };

        SearchResults<SearchDocument> response = await _searchClient.SearchAsync<SearchDocument>(null, searchOptions);

        var results = new List<SearchResult>(DataCount);
        await foreach (var result in response.GetResultsAsync())
        {
            results.Add(new SearchResult
            {
                Title = result.Document["title"].ToString(),
                Category = result.Document["category"].ToString(),
                Content = result.Document["content"].ToString(),
                Score = result.Score
            });
        }

        if (results.Any())
        {
            return new OkObjectResult(results);

        }

        return new NotFoundResult();
    }

    private async Task<IReadOnlyList<float>> GenerateEmbeddingsAsync(string text)
    {
        var response = await _openAIClient.GetEmbeddingsAsync(EmbeddingsDeploymentName, new EmbeddingsOptions(text));
        return response.Value.Data[0].Embedding;
    }
}