using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.Models.DevOps;

public record Avatar(
    [property: JsonPropertyName("href")] string Href
);