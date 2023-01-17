using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Models;

public record AzureDevOpsGitRepositoryResponse(
        [property: JsonPropertyName("value")] IReadOnlyList<AzureDevOpsGitRepository> Repositories,
        [property: JsonPropertyName("count")] int Count
    ) : IHasCount;