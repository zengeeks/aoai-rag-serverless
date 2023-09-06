namespace CognitiveSearchSamples.Models;

/// <summary>
/// Cosmos DB に格納用のスキーマ
/// </summary>
/// <remarks>camelcase の json も deserialize 可能</remarks>
public record AzureInfo(string Id, string Category, string Title, string Content);