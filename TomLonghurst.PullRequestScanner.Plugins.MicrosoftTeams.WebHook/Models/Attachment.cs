using Newtonsoft.Json;

namespace TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Models;

internal class Attachment
{
    [JsonProperty("contentType")]
    public string ContentType { get; set; }
        
    [JsonProperty("content")]
    public MicrosoftTeamsAdaptiveCard Content { get; set; }
}