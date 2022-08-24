using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.Models.DevOps;

public record DevOpsTeamMembersResponseWrapper(
    [property: JsonPropertyName("value")] IReadOnlyList<DevOpsTeamMember> Value,
    [property: JsonPropertyName("count")] int Count
) : IHasCount;