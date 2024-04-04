namespace TomLonghurst.PullRequestScanner.AzureDevOps.Services;

using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using Options;

internal class AzureDevOpsGitRepositoryService(VssConnection vssConnection, AzureDevOpsOptions azureDevOpsOptions) : IAzureDevOpsGitRepositoryService
{
    private readonly VssConnection vssConnection = vssConnection;
    private readonly AzureDevOpsOptions azureDevOpsOptions = azureDevOpsOptions;

    public async Task<List<GitRepository>> GetGitRepositories()
    {
        var repositories = await vssConnection.GetClient<GitHttpClient>().GetRepositoriesAsync(azureDevOpsOptions.ProjectGuid);

        return repositories.Where(x => x.IsDisabled != true)
            .Where(x => azureDevOpsOptions.RepositoriesToScan.Invoke(x))
            .ToList();
    }
}