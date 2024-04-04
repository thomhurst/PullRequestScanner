namespace TomLonghurst.PullRequestScanner.GitHub.Models.GraphQl;

using System.Text.Json.Serialization;

public record Repository(
    [property: JsonPropertyName("pullRequest")] PullRequest PullRequest);