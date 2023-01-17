using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Models;

public record DevOpsCommit(
    [property: JsonPropertyName("committer")] DevOpsCommitter Committer
    );