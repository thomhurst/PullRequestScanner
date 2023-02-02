using TomLonghurst.PullRequestScanner.AzureDevOps.Http;
using TomLonghurst.PullRequestScanner.AzureDevOps.Models;
using TomLonghurst.PullRequestScanner.AzureDevOps.Options;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Services;

internal class AzureDevOpsGitRepositoryService : IAzureDevOpsGitRepositoryService
{
    private readonly AzureDevOpsHttpClient _githubHttpClient;
    private readonly AzureDevOpsOptions _azureDevOpsOptions;

    public AzureDevOpsGitRepositoryService(AzureDevOpsHttpClient githubHttpClient, AzureDevOpsOptions azureDevOpsOptions)
    {
        _githubHttpClient = githubHttpClient;
        _azureDevOpsOptions = azureDevOpsOptions;
    }
    
    public async Task<IEnumerable<AzureDevOpsGitRepository>> GetGitRepositories()
    {
        var gitRepositoryResponse = await _githubHttpClient.GetAll<AzureDevOpsGitRepositoryResponse>("repositories?api-version=7.1-preview.1");
        return gitRepositoryResponse.SelectMany(x => x.Repositories)
            .Where(x => !x.IsDisabled)
            .Where(x => _azureDevOpsOptions.RepositoriesToScan.Invoke(x));
    }
}