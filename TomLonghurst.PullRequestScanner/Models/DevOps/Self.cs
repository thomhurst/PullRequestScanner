using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.Models.DevOps;

public record Self(
    [property: JsonPropertyName("href")] string Href
);