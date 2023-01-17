using Newtonsoft.Json;

namespace TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Models;

internal class TeamsNotificationCardWrapper
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("attachments")]
    public Attachment[] Attachments { get; set; }

    public static TeamsNotificationCardWrapper Wrap(MicrosoftTeamsAdaptiveCard adaptiveCard)
    {
        return new TeamsNotificationCardWrapper
        {
            Type = "message",
            Attachments = new[]
            {
                new Attachment
                {
                    ContentType = "application/vnd.microsoft.card.adaptive",
                    Content = adaptiveCard
                }
            }
        };
    }
}