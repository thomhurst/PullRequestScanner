namespace TomLonghurst.PullRequestScanner.AzureDevOps.Services;

using Initialization.Microsoft.Extensions.DependencyInjection;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using TomLonghurst.PullRequestScanner.AzureDevOps.Options;

public class AzureDevOpsInitializer(AzureDevOpsOptions azureDevOpsOptions, VssConnection vssConnection) : IInitializer
{
    private readonly AzureDevOpsOptions azureDevOpsOptions = azureDevOpsOptions;
    private readonly VssConnection vssConnection = vssConnection;

    public async Task InitializeAsync()
    {
        if (this.azureDevOpsOptions.ProjectGuid != default)
        {
            return;
        }

        var projects = await this.GetProjects();

        var foundProject = projects.SingleOrDefault(x => string.Equals(x.Name, this.azureDevOpsOptions.ProjectName, StringComparison.OrdinalIgnoreCase)) ?? throw new ArgumentException($"Unique project with name '{this.azureDevOpsOptions.ProjectName}' not found");
        this.azureDevOpsOptions.ProjectGuid = foundProject.Id;
    }

    private async Task<List<TeamProjectReference>> GetProjects()
    {
        var projects = new List<TeamProjectReference>();

        string continuationToken;
        do
        {
            var projectsInIteration = await this.vssConnection.GetClient<ProjectHttpClient>().GetProjects();

            projects.AddRange(projectsInIteration);

            continuationToken = projectsInIteration.ContinuationToken;
        }
        while (!string.IsNullOrEmpty(continuationToken));

        return projects;
    }
}