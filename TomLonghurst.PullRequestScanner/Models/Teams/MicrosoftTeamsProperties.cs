using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace TomLonghurst.PullRequestScanner.Models.Teams;

internal class MicrosoftTeamsProperties
{
    [JsonProperty("width")]
    public string Width { get; set; }

    [JsonProperty("entities")]
    public AdaptiveCardMentionedEntity[] Entitities { get; set; }
}

internal record Mentioned(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name
);

internal record AdaptiveCardMentionedEntity(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("mentioned")]
    Mentioned Mentioned
);
