using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Models;

public record DevOpsPullRequestThreadProperties(
    [property: JsonPropertyName("CodeReviewThreadType")] DevOpsPullRequestThreadPropertyValue? CodeReviewThreadType
);