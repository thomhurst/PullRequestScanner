namespace TomLonghurst.PullRequestScanner.GitHub.Models.GraphQl;

using System.Text.Json.Serialization;

public record Edge(
    [property: JsonPropertyName("node")] CommentThreadNode CommentThreadNode);