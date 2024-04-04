namespace TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Models;

using Newtonsoft.Json;

internal class MicrosoftTeamsProperties
{
    [JsonProperty("width")]
    public string Width { get; set; }

    [JsonProperty("entities")]
    public AdaptiveCardMentionedEntity[] Entitities { get; set; }
}