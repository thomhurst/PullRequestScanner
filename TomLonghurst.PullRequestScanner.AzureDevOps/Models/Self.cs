using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Models;

public record Self(
    [property: JsonPropertyName("href")] string Href
);