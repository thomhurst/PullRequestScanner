using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Models;

public record AzureDevOpsPullRequestIterationContext(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("genre")] string Genre
);