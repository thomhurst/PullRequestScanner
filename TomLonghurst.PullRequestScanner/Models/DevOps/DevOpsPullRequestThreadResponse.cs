using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.Models.DevOps;

public record DevOpsPullRequestThreadResponse(
        [property: JsonPropertyName("value")] IReadOnlyList<DevOpsPullRequestThread> Threads,
        [property: JsonPropertyName("count")] int Count
    );