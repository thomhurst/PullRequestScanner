using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Models;

public record AzureDevOpsPullRequestThreadProperties(
    [property: JsonPropertyName("CodeReviewThreadType")] AzureDevOpsPullRequestThreadPropertyValue? CodeReviewThreadType
);