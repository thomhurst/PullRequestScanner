using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Models;

public record AzureDevOpsTeamWrapper(
    [property: JsonPropertyName("value")] IReadOnlyList<AzureDevOpsTeam> Value,
    [property: JsonPropertyName("count")] int Count
) : IHasCount;