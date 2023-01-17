using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Models;

public record AzureDevOpsPullRequestIterationResponse(
    [property: JsonPropertyName("value")] IReadOnlyList<AzureDevOpsPullRequestIteration> Value,
    [property: JsonPropertyName("count")] int Count
) : IHasCount;