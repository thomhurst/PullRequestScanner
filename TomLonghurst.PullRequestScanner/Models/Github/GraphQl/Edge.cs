using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.Models.Github.GraphQl;

public record Edge(
    [property: JsonPropertyName("node")] CommentThreadNode CommentThreadNode
);