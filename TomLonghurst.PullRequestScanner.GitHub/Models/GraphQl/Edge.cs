using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.GitHub.Models.GraphQl;

public record Edge(
    [property: JsonPropertyName("node")] CommentThreadNode CommentThreadNode
);