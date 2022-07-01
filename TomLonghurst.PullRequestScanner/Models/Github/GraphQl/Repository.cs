using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.Models.Github.GraphQl;

public record Repository(
    [property: JsonPropertyName("pullRequest")] PullRequest PullRequest
);