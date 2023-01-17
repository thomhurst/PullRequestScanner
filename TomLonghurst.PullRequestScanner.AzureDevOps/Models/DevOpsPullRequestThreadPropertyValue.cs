using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Models;

public record AzureDevOpsPullRequestThreadPropertyValue(
    [property: JsonPropertyName("$value")] string? Value
);