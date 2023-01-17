using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Models;

public record AzureDevOpsPullRequestIteration(
    [property: JsonPropertyName("iterationId")] int IterationId,
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("state")] string State,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("context")] AzureDevOpsPullRequestIterationContext Context
);