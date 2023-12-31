namespace CognitiveSearchSamples.Models;

/// <summary>
/// 検索結果のモデル
/// </summary>
/// <remarks>検索結果の API の response body には List で出力</remarks>>
public class SearchResult
{
    public string Title { get; set; }
    public string Category { get; set; }
    public string Content { get; set; }
    public double? Score { get; set; }
}