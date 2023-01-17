using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Models;

public record DevOpsPullRequestThreadPropertyValue(
    [property: JsonPropertyName("$value")] string? Value
);