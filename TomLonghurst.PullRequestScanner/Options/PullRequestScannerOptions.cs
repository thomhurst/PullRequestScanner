namespace TomLonghurst.PullRequestScanner.Options;

public class PullRequestScannerOptions
{
    /**
     * <summary>Github Options</summary>
     */
    public GithubOptions Github { get; set; } = new GithubOrganizationTeamOptions
    {
        IsEnabled = false
    };

    /**
     * <summary>Azure DevOps Options</summary>
     */
    public AzureDevOpsOptions AzureDevOps { get; set; } = new()
    {
        IsEnabled = false
    };

    /**
     * <summary>Microsoft Teams Options</summary>
     */
    public MicrosoftTeamsOptions MicrosoftTeams { get; set; } = new();
}