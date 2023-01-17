using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Models;

public record AzureDevOpsCommitter(
    [property: JsonPropertyName("date")] DateTime Date,
    [property: JsonPropertyName("name")] string Name
);