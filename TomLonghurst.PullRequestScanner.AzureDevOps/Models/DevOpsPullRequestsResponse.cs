using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Models;

public record DevOpsPullRequestsResponse(
        [property: JsonPropertyName("value")] IReadOnlyList<DevOpsPullRequest> PullRequests,
        [property: JsonPropertyName("count")] int Count
    ) : IHasCount;