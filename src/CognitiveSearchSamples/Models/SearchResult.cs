namespace CognitiveSearchSamples.Models;

/// <summary>
/// �������ʂ̃��f��
/// </summary>
/// <remarks>�������ʂ� API �� response body �ɂ� List �ŏo��</remarks>>
public class SearchResult
{
    public string Title { get; set; }
    public string Category { get; set; }
    public string Content { get; set; }
    public double? Score { get; set; }
}