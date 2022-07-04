namespace TomLonghurst.PullRequestScanner.Options;

public class PullRequestScannerOptions
{
    public GithubOptions Github { get; set; } = new GithubOrganizationTeamOptions
    {
        IsEnabled = false
    };

    public AzureDevOpsOptions AzureDevOps { get; set; } = new()
    {
        IsEnabled = false
    };

    public MicrosoftTeamsOptions MicrosoftTeams { get; set; } = new();
}