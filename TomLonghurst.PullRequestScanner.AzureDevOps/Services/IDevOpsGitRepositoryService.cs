namespace TomLonghurst.PullRequestScanner.AzureDevOps.Services;

using Microsoft.TeamFoundation.SourceControl.WebApi;

internal interface IAzureDevOpsGitRepositoryService
{
    Task<List<GitRepository>> GetGitRepositories();
}