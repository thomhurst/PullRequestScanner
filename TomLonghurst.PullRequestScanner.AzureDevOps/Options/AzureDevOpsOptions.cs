using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Options;

public class AzureDevOpsOptions
{
    public bool IsEnabled { get; set; } = true;
    /**
     * <summary>The organization name</summary>
     */
    public string Organization { get; set; }
    /**
     * <summary>The project name</summary>
     */
    public string ProjectName { get; set; }
    /**
     * <summary>The project GUID</summary>
     */
    public Guid ProjectGuid { get; set; }
    /**
     * <summary>An Azure DevOps Personal Access Token</summary>
     */
    public string PersonalAccessToken { get; set; }

    public Func<GitRepository, bool> RepositoriesToScan { get; set; } = _ => true;
}