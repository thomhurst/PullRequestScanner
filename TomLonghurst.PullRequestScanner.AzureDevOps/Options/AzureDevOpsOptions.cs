using TomLonghurst.PullRequestScanner.AzureDevOps.Models;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Options;

public class AzureDevOpsOptions
{
    public bool IsEnabled { get; set; } = true;
    /**
     * <summary>The URL slug for the organization</summary>
     */
    public string OrganizationSlug { get; set; }
    /**
     * <summary>The URL slug for the team</summary>
     */
    public string ProjectSlug { get; set; }
    /**
     * <summary>Personal Access Token, usually in the format of "{username}:{PAT}"</summary>
     */
    public string PersonalAccessToken { get; set; }

    public Func<AzureDevOpsGitRepository, bool> RepositoriesToScan { get; set; } = _ => true;
}