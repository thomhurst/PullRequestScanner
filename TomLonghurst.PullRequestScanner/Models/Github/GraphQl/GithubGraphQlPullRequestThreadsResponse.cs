using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.Models.Github.GraphQl;

public record GithubGraphQlPullRequestThreadsResponse(
    [property: JsonPropertyName("data")] Data Data
);

