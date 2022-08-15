using Newtonsoft.Json;

namespace TomLonghurst.PullRequestScanner.Models.Teams;

internal class MicrosoftTeamsProperties
{
    [JsonProperty("width")]
    public string Width { get; set; }

    [JsonProperty("entities")]
    public AdaptiveCardMentionedEntity[] Entitities { get; set; }
}