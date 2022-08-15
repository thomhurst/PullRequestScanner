using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.Models.Teams;

internal record Mentioned(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name
);