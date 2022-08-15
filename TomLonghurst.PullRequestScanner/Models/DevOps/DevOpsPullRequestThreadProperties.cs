using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.Models.DevOps;

public record DevOpsPullRequestThreadProperties(
    [property: JsonPropertyName("CodeReviewThreadType")] DevOpsPullRequestThreadPropertyValue? CodeReviewThreadType
);