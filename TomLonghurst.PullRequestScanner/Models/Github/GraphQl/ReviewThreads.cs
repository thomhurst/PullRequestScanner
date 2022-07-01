using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.Models.Github.GraphQl;

public record ReviewThreads(
    [property: JsonPropertyName("edges")] IReadOnlyList<Edge> Edges
);