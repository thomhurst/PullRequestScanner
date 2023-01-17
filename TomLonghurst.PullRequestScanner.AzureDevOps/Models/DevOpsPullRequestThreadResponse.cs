using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Models;

public record DevOpsPullRequestThreadResponse(
        [property: JsonPropertyName("value")] IReadOnlyList<DevOpsPullRequestThread> Threads,
        [property: JsonPropertyName("count")] int Count
    ) : IHasCount;