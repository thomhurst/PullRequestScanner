using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.GitHub.Models.GraphQl;

public record Author(
    [property: JsonPropertyName("login")] string Login
);