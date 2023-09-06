using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace CosmosDataUploader;

public class Function1
{
    private readonly CosmosClient _cosmosClient;

    public Function1(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
    }

    [FunctionName(nameof(UploadAzureInfo))]
    public async Task<IActionResult> UploadAzureInfo(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null), FromBody] AzureInfo[] items,
        ILogger log)
    {
        var container = _cosmosClient.GetContainer("Aoai", "AzureInfo");

        var tasks = items.Select(item => container.UpsertItemAsync(item, new PartitionKey(item.Id))).ToArray();
        await Task.WhenAll(tasks);

        return new OkResult();

        // データ更新のユースケース次第では Bulk での Insert もあり。
        // 参考: https://learn.microsoft.com/ja-jp/azure/cosmos-db/nosql/how-to-migrate-from-bulk-executor-library
    }
}

public record AzureInfo(string Id, string Category, string Title, string Content);