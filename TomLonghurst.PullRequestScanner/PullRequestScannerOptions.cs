namespace TomLonghurst.PullRequestScanner;

public class PullRequestScannerOptions
{
    public PullRequestGithubOptions Github { get; set; } = new()
    {
        IsEnabled = false
    };

    public PullRequestAzureDevOpsOptions AzureDevOps { get; set; } = new()
    {
        IsEnabled = false
    };

    public MicrosoftTeamsOptions MicrosoftTeams { get; set; } = new()
    {
        IsEnabled = false
    };
}