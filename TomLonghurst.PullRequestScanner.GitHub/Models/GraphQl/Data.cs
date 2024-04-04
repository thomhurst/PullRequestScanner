namespace TomLonghurst.PullRequestScanner.GitHub.Models.GraphQl;

using System.Text.Json.Serialization;

public record Data(
    [property: JsonPropertyName("repository")] Repository Repository);