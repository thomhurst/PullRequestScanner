using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Models;

public record AzureDevOpsPullRequestThread(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("comments")] IReadOnlyList<AzureDevOpsComment> Comments,
    [property: JsonPropertyName("status")] string ThreadStatus,
    [property: JsonPropertyName("isDeleted")] bool IsDeleted,
    [property: JsonPropertyName("publishedDate")] DateTime PublishedDate,
    [property: JsonPropertyName("lastUpdatedDate")] DateTime LastUpdatedDate,
    [property: JsonPropertyName("properties")] AzureDevOpsPullRequestThreadProperties Properties
);