// <copyright file="DevOpsGitRepositoryService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Services;

using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using TomLonghurst.PullRequestScanner.AzureDevOps.Options;

internal class AzureDevOpsGitRepositoryService(VssConnection vssConnection, AzureDevOpsOptions azureDevOpsOptions) : IAzureDevOpsGitRepositoryService
{
    private readonly VssConnection vssConnection = vssConnection;
    private readonly AzureDevOpsOptions azureDevOpsOptions = azureDevOpsOptions;

    public async Task<List<GitRepository>> GetGitRepositories()
    {
        var repositories = await this.vssConnection.GetClient<GitHttpClient>().GetRepositoriesAsync(this.azureDevOpsOptions.ProjectGuid);

        return repositories.Where(x => x.IsDisabled != true)
            .Where(x => this.azureDevOpsOptions.RepositoriesToScan.Invoke(x))
            .ToList();
    }
}