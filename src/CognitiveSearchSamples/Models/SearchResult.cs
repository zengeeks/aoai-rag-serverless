namespace CognitiveSearchSamples.Models;

/// <summary>
/// ŒŸõŒ‹‰Ê‚Ìƒ‚ƒfƒ‹
/// </summary>
/// <remarks>ŒŸõŒ‹‰Ê‚Ì API ‚Ì response body ‚É‚Í List ‚Åo—Í</remarks>>
public class SearchResult
{
    public string Title { get; set; }
    public string Category { get; set; }
    public string Content { get; set; }
    public double? Score { get; set; }
}