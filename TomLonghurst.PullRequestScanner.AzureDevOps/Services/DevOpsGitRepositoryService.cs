using TomLonghurst.PullRequestScanner.AzureDevOps.Http;
using TomLonghurst.PullRequestScanner.AzureDevOps.Models;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Services;

internal class AzureDevOpsGitRepositoryService : IAzureDevOpsGitRepositoryService
{
    private readonly AzureDevOpsHttpClient _githubHttpClient;

    public AzureDevOpsGitRepositoryService(AzureDevOpsHttpClient githubHttpClient)
    {
        _githubHttpClient = githubHttpClient;
    }
    
    public async Task<IEnumerable<AzureDevOpsGitRepository>> GetGitRepositories()
    {
        var gitRepositoryResponse = await _githubHttpClient.GetAll<AzureDevOpsGitRepositoryResponse>("repositories?api-version=7.1-preview.1");
        return gitRepositoryResponse.SelectMany(x => x.Repositories).Where(x => !x.IsDisabled);
    }
}