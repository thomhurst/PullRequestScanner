using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Models;

public record DevOpsTeamWrapper(
    [property: JsonPropertyName("value")] IReadOnlyList<DevOpsTeam> Value,
    [property: JsonPropertyName("count")] int Count
) : IHasCount;