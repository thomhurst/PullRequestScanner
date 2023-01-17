using TomLonghurst.PullRequestScanner.AzureDevOps.Http;
using TomLonghurst.PullRequestScanner.AzureDevOps.Models;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Services;

internal class DevOpsGitRepositoryService : IDevOpsGitRepositoryService
{
    private readonly DevOpsHttpClient _githubHttpClient;

    public DevOpsGitRepositoryService(DevOpsHttpClient githubHttpClient)
    {
        _githubHttpClient = githubHttpClient;
    }
    
    public async Task<IEnumerable<DevOpsGitRepository>> GetGitRepositories()
    {
        var gitRepositoryResponse = await _githubHttpClient.GetAll<DevOpsGitRepositoryResponse>("repositories?api-version=7.1-preview.1");
        return gitRepositoryResponse.SelectMany(x => x.Repositories).Where(x => !x.IsDisabled);
    }
}