using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.Models.Github.GraphQl;

public record Data(
    [property: JsonPropertyName("repository")] Repository Repository
);