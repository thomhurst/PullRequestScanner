using TomLonghurst.PullRequestScanner.AzureDevOps.Models;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Services;

internal interface IAzureDevOpsGitRepositoryService
{
    Task<IEnumerable<AzureDevOpsGitRepository>> GetGitRepositories();
}