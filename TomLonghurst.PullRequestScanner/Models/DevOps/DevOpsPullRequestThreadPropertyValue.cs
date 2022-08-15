using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.Models.DevOps;

public record DevOpsPullRequestThreadPropertyValue(
    [property: JsonPropertyName("$value")] string? Value
);