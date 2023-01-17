using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.GitHub.Models.GraphQl;

public record Data(
    [property: JsonPropertyName("repository")] Repository Repository
);