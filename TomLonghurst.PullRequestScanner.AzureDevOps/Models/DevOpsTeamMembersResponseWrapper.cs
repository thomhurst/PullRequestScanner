using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Models;

public record DevOpsTeamMembersResponseWrapper(
    [property: JsonPropertyName("value")] IReadOnlyList<DevOpsTeamMember> Value,
    [property: JsonPropertyName("count")] int Count
) : IHasCount;