namespace TomLonghurst.PullRequestScanner.GitHub.Models.GraphQl;

using System.Text.Json.Serialization;

public record Author(
    [property: JsonPropertyName("login")] string Login);