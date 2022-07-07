using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.Models.DevOps;

public record DevOpsCommitter(
    [property: JsonPropertyName("date")] DateTime Date,
    [property: JsonPropertyName("name")] string Name
);