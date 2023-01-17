namespace TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Options;

public class MicrosoftTeamsOptions
{
    /**
     * <summary>The webhook URL to send Teams Cards to</summary>
     */
    public Uri? WebHookUri { get; set; }

    public MicrosoftTeamsPublishOptions PublishOptions { get; set; } = new();
}