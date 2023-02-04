using TomLonghurst.PullRequestScanner.AzureDevOps.Extensions;
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
        List<AzureDevOpsGitRepository> repositories = new List<AzureDevOpsGitRepository>();

        foreach (string project in _azureDevOpsOptions.GetProjects())
        {
            await foreach (var repository in _githubHttpClient.ListGitRepositories(project))
            {
                if (repository.IsDisabled)
                {
                    continue;
                }

                bool includeRepository = _azureDevOpsOptions.RepositoriesToScan(repository);

                if (!includeRepository)
                {
                    continue;
                }

                repositories.Add(repository);
            }
        }

        return repositories;
    }
}