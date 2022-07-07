using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.Models.DevOps;

public record Links(
    [property: JsonPropertyName("avatar")] Avatar Avatar
);