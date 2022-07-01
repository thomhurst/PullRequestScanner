using TomLonghurst.PullRequestScanner.Http;
using TomLonghurst.PullRequestScanner.Models.DevOps;

namespace TomLonghurst.PullRequestScanner.Services.DevOps;

internal class DevOpsGitRepositoryService : IDevOpsGitRepositoryService
{
    private readonly DevOpsHttpClient _githubHttpClient;

    public DevOpsGitRepositoryService(DevOpsHttpClient githubHttpClient)
    {
        _githubHttpClient = githubHttpClient;
    }
    
    public async Task<IEnumerable<DevOpsGitRepository>> GetGitRepositories()
    {
        var gitRepositoryResponse = await _githubHttpClient.Get<DevOpsGitRepositoryResponse>("repositories?api-version=7.1-preview.1");
        return gitRepositoryResponse.Repositories.Where(x => !x.IsDisabled);
    }
}