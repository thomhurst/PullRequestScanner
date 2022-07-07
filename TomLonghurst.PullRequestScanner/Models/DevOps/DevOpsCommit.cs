using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.Models.DevOps;

public record DevOpsCommit(
    [property: JsonPropertyName("committer")] DevOpsCommitter Committer
    );