namespace CognitiveSearchSamples.Models;

/// <summary>
/// Cosmos DB �Ɋi�[�p�̃X�L�[�}
/// </summary>
/// <remarks>camelcase �� json �� deserialize �\</remarks>
public record AzureInfo(string Id, string Category, string Title, string Content);