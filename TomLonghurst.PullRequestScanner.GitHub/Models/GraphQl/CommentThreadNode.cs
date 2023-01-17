using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.GitHub.Models.GraphQl;

public record CommentThreadNode(
    [property: JsonPropertyName("isResolved")] bool IsResolved,
    [property: JsonPropertyName("isOutdated")] bool IsOutdated,
    [property: JsonPropertyName("isCollapsed")] bool IsCollapsed,
    [property: JsonPropertyName("comments")] Comments Comments
);