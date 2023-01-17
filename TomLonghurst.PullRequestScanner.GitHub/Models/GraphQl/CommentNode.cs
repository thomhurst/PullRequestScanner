using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.GitHub.Models.GraphQl;

public record CommentNode(
    [property: JsonPropertyName("author")] Author Author,
    [property: JsonPropertyName("body")] string Body,
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("updatedAt")] DateTime UpdatedAt
);