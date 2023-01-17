using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.GitHub.Models.GraphQl;

public record Repository(
    [property: JsonPropertyName("pullRequest")] PullRequest PullRequest
);