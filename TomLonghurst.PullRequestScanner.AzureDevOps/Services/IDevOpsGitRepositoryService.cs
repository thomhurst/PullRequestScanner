using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Services;

internal interface IAzureDevOpsGitRepositoryService
{
    Task<List<GitRepository>> GetGitRepositories();
}