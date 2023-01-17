using TomLonghurst.PullRequestScanner.AzureDevOps.Models;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Services;

internal interface IDevOpsGitRepositoryService
{
    Task<IEnumerable<DevOpsGitRepository>> GetGitRepositories();
}