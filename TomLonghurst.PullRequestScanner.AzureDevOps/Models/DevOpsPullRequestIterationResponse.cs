using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Models;

public record DevOpsPullRequestIterationResponse(
    [property: JsonPropertyName("value")] IReadOnlyList<DevOpsPullRequestIteration> Value,
    [property: JsonPropertyName("count")] int Count
) : IHasCount;