using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.Models.DevOps;

public record DevOpsPullRequestThread(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("comments")] IReadOnlyList<DevOpsComment> Comments,
    [property: JsonPropertyName("status")] string ThreadStatus,
    [property: JsonPropertyName("isDeleted")] bool IsDeleted
);