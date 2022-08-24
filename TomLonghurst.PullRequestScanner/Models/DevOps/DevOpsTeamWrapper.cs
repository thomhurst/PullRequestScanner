using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.Models.DevOps;

public record DevOpsTeamWrapper(
    [property: JsonPropertyName("value")] IReadOnlyList<DevOpsTeam> Value,
    [property: JsonPropertyName("count")] int Count
) : IHasCount;