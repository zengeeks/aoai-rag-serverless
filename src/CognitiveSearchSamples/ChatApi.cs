using Azure.AI.OpenAI;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CognitiveSearchSamples.Models;

namespace CognitiveSearchSamples;
public class ChatApi
{
    private static readonly string EmbeddingsDeploymentName = "text-embedding-ada-002";
    private static readonly string ChatGptDeploymentName = "gpt-35-turbo";
    private static readonly string VectorField = "contentVector";
    private static readonly int DataCount = 3;

    private readonly OpenAIClient _openAIClient;
    private readonly SearchClient _searchClient;

    public ChatApi(OpenAIClient openAIClient, SearchClient searchClient)
    {
        _openAIClient = openAIClient;
        _searchClient = searchClient;
    }

    [FunctionName("chat")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequest req,
        ILogger log)
    {
        var query = req.Query["query"];

        if (string.IsNullOrWhiteSpace(query))
        {
            return new BadRequestResult();
        }

        var vectorSearchResults = await VectorSearchAsync(query);
        var answer = await GenerateAnswerAsync(query, vectorSearchResults, log);

        return new OkObjectResult(answer);
    }

    #region Vector search

    private async Task<List<SearchResult>> VectorSearchAsync(string query)
    {
        // �x�N�^�[��
        var embedding = await GetEmbeddingAsync(query);
        // �x�N�^�[�����̎��s
        var searchOptions = new SearchOptions
        {
            Vectors = { new SearchQueryVector { Value = embedding.ToArray(), KNearestNeighborsCount = 3, Fields = { VectorField } } },
            Size = DataCount,
            Select = { "title", "content", "category" }
        };

        // `Azure.Search.Documents.Models.SearchDocument` �� `SearchAsync()` �� Generics �ɒ�`���ăR�[�����Ă��邪�A
        // �Ǝ��� class �� Generics �Ƃ��Ē�`���\�B
        SearchResults<SearchDocument> response = await _searchClient.SearchAsync<SearchDocument>(null, searchOptions);
        var searchResults = new List<SearchResult>(DataCount);
        await foreach (var result in response.GetResultsAsync())
        {
            searchResults.Add(new SearchResult
            {
                Title = result.Document["title"].ToString(),
                Category = result.Document["category"].ToString(),
                Content = result.Document["content"].ToString(),
                Score = result.Score
            });
        }

        return searchResults;
    }

    private async Task<IReadOnlyList<float>> GetEmbeddingAsync(string text)
    {
        var response = await _openAIClient.GetEmbeddingsAsync(EmbeddingsDeploymentName, new EmbeddingsOptions(text));
        return response.Value.Data[0].Embedding;
    }

    #endregion Vector search

    #region Generate answer

    private async Task<string> GenerateAnswerAsync(string question, List<SearchResult> documents, ILogger log)
    {
        var json = JsonSerializer.Serialize(documents);
        log.LogInformation($"Vector search result:\n{json}");

        var options = new ChatCompletionsOptions
        {
            Temperature = 0,
            Messages =
                {
                    new ChatMessage(ChatRole.System, @$"���Ȃ��� Microsoft Azure �̃T�[�r�X���Љ����Ƃł��B
Microsoft Azure �Ɋւ��鎿��ȊO�ɂ́uMicrosoft Azure �Ɋ֘A���Ȃ�����ɂ͂������ł��܂���B�v�Ɖ񓚂��܂��B
�񓚂́A�ȉ��� references tag �̒��� JSON ����񓚂��Ă��������B�Ó��ȉ񓚂��݂���Ȃ��ꍇ�͂킩��Ȃ��ƕԓ����Ă��������B

<references>
{json}
</references>

�񓚂́A�T�[�r�X���Ƃ��̐������܂߂ĊȌ��ɂ܂Ƃ߂Ă��������B"),
                    new ChatMessage(ChatRole.User, question)
                }
        };

        var response = await _openAIClient.GetChatCompletionsAsync(ChatGptDeploymentName, options);

        log.LogInformation($"TotalTokens: {response.Value.Usage.TotalTokens} (CompletionTokens: {response.Value.Usage.CompletionTokens}; PromptTokens: {response.Value.Usage.PromptTokens})");
        log.LogInformation("Answers:");
        foreach (var choice in response.Value.Choices)
        {
            log.LogInformation(choice.Message.Content);
        }

        log.LogInformation("----");

        return response.Value.Choices.First().Message.Content;
    }


    #endregion Generate answer
}
