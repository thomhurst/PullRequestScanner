using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.GitHub.Models.GraphQl;

public record ReviewThreads(
    [property: JsonPropertyName("edges")] IReadOnlyList<Edge> Edges
);