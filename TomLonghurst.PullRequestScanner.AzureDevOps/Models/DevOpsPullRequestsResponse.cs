using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Models;

public record AzureDevOpsPullRequestsResponse(
        [property: JsonPropertyName("value")] IReadOnlyList<AzureDevOpsPullRequest> PullRequests,
        [property: JsonPropertyName("count")] int Count
    ) : IHasCount;