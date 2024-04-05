namespace TomLonghurst.PullRequestScanner.GitHub.Models.GraphQl;

using System.Text.Json.Serialization;

public record ReviewThreads(
    [property: JsonPropertyName("edges")] IReadOnlyList<Edge> Edges);