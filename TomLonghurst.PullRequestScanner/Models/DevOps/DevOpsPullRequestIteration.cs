using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.Models.DevOps;

public record DevOpsPullRequestIteration(
    [property: JsonPropertyName("iterationId")] int IterationId,
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("state")] string State,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("context")] DevOpsPullRequestIterationContext Context
);

public record DevOpsPullRequestIterationContext(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("genre")] string Genre
);