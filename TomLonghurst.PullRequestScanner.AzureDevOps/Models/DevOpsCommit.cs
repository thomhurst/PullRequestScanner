using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Models;

public record AzureDevOpsCommit(
    [property: JsonPropertyName("committer")] AzureDevOpsCommitter Committer
    );