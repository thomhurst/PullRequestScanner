using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.Models.DevOps;

public record DevOpsPullRequestThread(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("comments")] IReadOnlyList<DevOpsComment> Comments,
    [property: JsonPropertyName("status")] string ThreadStatus,
    [property: JsonPropertyName("isDeleted")] bool IsDeleted,
    [property: JsonPropertyName("publishedDate")] DateTime PublishedDate,
    [property: JsonPropertyName("lastUpdatedDate")] DateTime LastUpdatedDate,
    [property: JsonPropertyName("properties")] DevOpsPullRequestThreadProperties Properties
);

public record DevOpsPullRequestThreadProperties(
    [property: JsonPropertyName("CodeReviewThreadType")] DevOpsPullRequestThreadPropertyValue? CodeReviewThreadType
);

public record DevOpsPullRequestThreadPropertyValue(
    [property: JsonPropertyName("$value")] string? Value
    );