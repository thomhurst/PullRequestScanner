using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Models;

public record DevOpsPullRequestThread(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("comments")] IReadOnlyList<DevOpsComment> Comments,
    [property: JsonPropertyName("status")] string ThreadStatus,
    [property: JsonPropertyName("isDeleted")] bool IsDeleted,
    [property: JsonPropertyName("publishedDate")] DateTime PublishedDate,
    [property: JsonPropertyName("lastUpdatedDate")] DateTime LastUpdatedDate,
    [property: JsonPropertyName("properties")] DevOpsPullRequestThreadProperties Properties
);