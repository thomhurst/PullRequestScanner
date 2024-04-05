namespace TomLonghurst.PullRequestScanner.GitHub.Models.GraphQl;

using System.Text.Json.Serialization;

public record PullRequest(
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("reviewDecision")] string ReviewDecision,
    [property: JsonPropertyName("reviewThreads")] ReviewThreads ReviewThreads);