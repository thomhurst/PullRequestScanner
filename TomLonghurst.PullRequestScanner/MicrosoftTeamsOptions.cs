namespace TomLonghurst.PullRequestScanner;

public class MicrosoftTeamsOptions
{
    public bool IsEnabled { get; set; } = true;
    public Uri? WebHookUri { get; set; }
}