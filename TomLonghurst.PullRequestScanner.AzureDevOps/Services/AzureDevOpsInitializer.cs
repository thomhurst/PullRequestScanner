using Initialization.Microsoft.Extensions.DependencyInjection;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using TomLonghurst.PullRequestScanner.AzureDevOps.Options;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Services;

public class AzureDevOpsInitializer(AzureDevOpsOptions azureDevOpsOptions, VssConnection vssConnection) : IInitializer
{
    private readonly AzureDevOpsOptions _azureDevOpsOptions = azureDevOpsOptions;
    private readonly VssConnection _vssConnection = vssConnection;

    public async Task InitializeAsync()
    {
        if (_azureDevOpsOptions.ProjectGuid != default)
        {
            return;
        }

        var projects = await GetProjects();

        var foundProject = projects.SingleOrDefault(x => string.Equals(x.Name, _azureDevOpsOptions.ProjectName, StringComparison.OrdinalIgnoreCase)) ?? throw new ArgumentException($"Unique project with name '{_azureDevOpsOptions.ProjectName}' not found");
        _azureDevOpsOptions.ProjectGuid = foundProject.Id;
    }

    private async Task<List<TeamProjectReference>> GetProjects()
    {
        var projects = new List<TeamProjectReference>();

        string continuationToken;
        do
        {
            var projectsInIteration = await _vssConnection.GetClient<ProjectHttpClient>().GetProjects();

            projects.AddRange(projectsInIteration);

            continuationToken = projectsInIteration.ContinuationToken;
        } while (!string.IsNullOrEmpty(continuationToken));

        return projects;
    }
}