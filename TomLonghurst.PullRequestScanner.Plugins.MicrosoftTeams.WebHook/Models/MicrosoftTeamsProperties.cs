using Newtonsoft.Json;

namespace TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Models;

internal class MicrosoftTeamsProperties
{
    [JsonProperty("width")]
    public string Width { get; set; }

    [JsonProperty("entities")]
    public AdaptiveCardMentionedEntity[] Entitities { get; set; }
}