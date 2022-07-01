using TomLonghurst.PullRequestScanner.Models.DevOps;

namespace TomLonghurst.PullRequestScanner.Services.DevOps;

internal interface IDevOpsGitRepositoryService
{
    Task<IEnumerable<DevOpsGitRepository>> GetGitRepositories();
}