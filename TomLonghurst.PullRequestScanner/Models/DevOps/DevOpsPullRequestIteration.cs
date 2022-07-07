using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.Models.DevOps;

public record DevOpsPullRequestIteration(
    [property: JsonPropertyName("iterationId")] int IterationId,
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("state")] string State 
);

