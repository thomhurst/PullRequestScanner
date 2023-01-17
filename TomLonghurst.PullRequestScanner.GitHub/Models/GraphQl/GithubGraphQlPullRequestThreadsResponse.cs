using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.GitHub.Models.GraphQl;

public record GithubGraphQlPullRequestThreadsResponse(
    [property: JsonPropertyName("data")] Data Data
);

