using System.Collections.Generic;

namespace CognitiveSearchSamples.Models;

/// <summary>
/// Cognitive Search の Index 登録用スキーマ
/// </summary>
public class AzureIndexDocument
{
    public string Id { get; set; }
    public string Category { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public IReadOnlyList<float> ContentVector { get; set; }
}