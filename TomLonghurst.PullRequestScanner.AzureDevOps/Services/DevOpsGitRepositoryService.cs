using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using TomLonghurst.PullRequestScanner.AzureDevOps.Options;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Services;

internal class AzureDevOpsGitRepositoryService : IAzureDevOpsGitRepositoryService
{
    private readonly VssConnection _vssConnection;
    private readonly AzureDevOpsOptions _azureDevOpsOptions;

    public AzureDevOpsGitRepositoryService(VssConnection vssConnection, AzureDevOpsOptions azureDevOpsOptions)
    {
        _vssConnection = vssConnection;
        _azureDevOpsOptions = azureDevOpsOptions;
    }
    
    public async Task<List<GitRepository>> GetGitRepositories()
    {
        var repositories = await _vssConnection.GetClient<GitHttpClient>().GetRepositoriesAsync(_azureDevOpsOptions.ProjectGuid);

        return repositories.Where(x => x.IsDisabled != true) 
            .Where(x => _azureDevOpsOptions.RepositoriesToScan.Invoke(x))
            .ToList();
    }
}