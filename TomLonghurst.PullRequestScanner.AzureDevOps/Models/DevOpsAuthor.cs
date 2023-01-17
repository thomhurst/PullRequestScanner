using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Models;

public record DevOpsAuthor(
    [property: JsonPropertyName("displayName")] string DisplayName,
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("uniqueName")] string UniqueName
);