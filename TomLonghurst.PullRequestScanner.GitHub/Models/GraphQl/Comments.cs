using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.GitHub.Models.GraphQl;

public record Comments(
    [property: JsonPropertyName("totalCount")] int TotalCount,
    [property: JsonPropertyName("nodes")] IReadOnlyList<CommentNode> Nodes
);