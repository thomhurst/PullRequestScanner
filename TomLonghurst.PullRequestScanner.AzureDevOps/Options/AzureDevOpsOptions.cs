namespace TomLonghurst.PullRequestScanner.AzureDevOps.Options;

using Microsoft.TeamFoundation.SourceControl.WebApi;

public class AzureDevOpsOptions
{
    public bool IsEnabled { get; set; } = true;
    /**
     * <summary>Gets or sets the organization name.</summary>
     */
    public string Organization { get; set; }
    /**
     * <summary>Gets or sets the project name.</summary>
     */
    public string ProjectName { get; set; }
    /**
     * <summary>Gets or sets the project GUID.</summary>
     */
    public Guid ProjectGuid { get; set; }
    /**
     * <summary>Gets or sets an Azure DevOps Personal Access Token.</summary>
     */
    public string PersonalAccessToken { get; set; }

    public Func<GitRepository, bool> RepositoriesToScan { get; set; } = _ => true;
}