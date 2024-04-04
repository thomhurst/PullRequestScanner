using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Models;

internal record Mentioned(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name
);