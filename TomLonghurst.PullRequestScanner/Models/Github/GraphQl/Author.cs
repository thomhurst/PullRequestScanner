using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.Models.Github.GraphQl;

public record Author(
    [property: JsonPropertyName("login")] string Login
);