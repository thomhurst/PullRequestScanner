using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.Models.Github;

// Root myDeserializedClass = JsonSerializer.Deserialize<List<Root>>(myJsonResponse);

public record GithubRepository(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("node_id")] string NodeId,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("full_name")] string FullName,
        [property: JsonPropertyName("owner")] Owner Owner,
        [property: JsonPropertyName("private")] bool Private,
        [property: JsonPropertyName("html_url")] string HtmlUrl,
        [property: JsonPropertyName("description")] string Description,
        [property: JsonPropertyName("fork")] bool Fork,
        [property: JsonPropertyName("url")] string Url,
        [property: JsonPropertyName("git_url")] string GitUrl,
        [property: JsonPropertyName("pulls_url")] string PullsUrl,
        [property: JsonPropertyName("teams_url")] string TeamsUrl,
        [property: JsonPropertyName("archived")] bool Archived,
        [property: JsonPropertyName("disabled")] bool Disabled,
        [property: JsonPropertyName("visibility")] string Visibility,
        [property: JsonPropertyName("pushed_at")] DateTime PushedAt,
        [property: JsonPropertyName("created_at")] DateTime CreatedAt,
        [property: JsonPropertyName("updated_at")] DateTime UpdatedAt
);

