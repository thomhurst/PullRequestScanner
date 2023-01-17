using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Models;

public record AzureDevOpsPullRequestThreadResponse(
        [property: JsonPropertyName("value")] IReadOnlyList<AzureDevOpsPullRequestThread> Threads,
        [property: JsonPropertyName("count")] int Count
    ) : IHasCount;