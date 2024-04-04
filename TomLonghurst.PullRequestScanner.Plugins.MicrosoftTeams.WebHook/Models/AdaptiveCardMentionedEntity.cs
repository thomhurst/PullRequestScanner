using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Models;

internal record AdaptiveCardMentionedEntity(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("mentioned")]
    Mentioned Mentioned
);