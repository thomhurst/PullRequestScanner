using TomLonghurst.EnumerableAsyncProcessor.Extensions;
using TomLonghurst.PullRequestScanner.AzureDevOps.Http;
using TomLonghurst.PullRequestScanner.AzureDevOps.Models;
using TomLonghurst.PullRequestScanner.AzureDevOps.Options;
using TomLonghurst.PullRequestScanner.Contracts;
using TomLonghurst.PullRequestScanner.Models;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Services;

internal class AzureDevOpsTeamMembersProvider : ITeamMembersProvider
{
    private readonly AzureDevOpsHttpClient _devOpsHttpClient;
    private readonly AzureDevOpsOptions _azureDevOpsOptions;

    public AzureDevOpsTeamMembersProvider(AzureDevOpsHttpClient AzureDevOpsHttpClient, AzureDevOpsOptions azureDevOpsOptions)
    {
        _devOpsHttpClient = AzureDevOpsHttpClient;
        _azureDevOpsOptions = azureDevOpsOptions;
    }

    public async Task<IEnumerable<ITeamMember>> GetTeamMembers()
    {
        if (!_azureDevOpsOptions.IsEnabled)
        {
            return Array.Empty<ITeamMember>();
        }

        var teamsInProject = await _devOpsHttpClient.GetAll<AzureDevOpsTeamWrapper>(
            $"https://dev.azure.com/{_azureDevOpsOptions.OrganizationSlug}/_apis/projects/{_azureDevOpsOptions.ProjectSlug}/teams?api-version=7.1-preview.2");

        var membersResponses = await teamsInProject
            .SelectMany(x => x.Value)
            .ToAsyncProcessorBuilder()
            .SelectAsync(x => _devOpsHttpClient.GetAll<AzureDevOpsTeamMembersResponseWrapper>(
                $"https://dev.azure.com/{_azureDevOpsOptions.OrganizationSlug}/_apis/projects/{_azureDevOpsOptions.ProjectSlug}/teams/{x.Id}/members?api-version=7.1-preview.2"))
            .ProcessInParallel(50, TimeSpan.FromSeconds(5));

        return membersResponses
            .SelectMany(x => x)
            .SelectMany(x => x.Value)
            .Where(x => x.Identity.DisplayName != Constants.VSTSDisplayName)
            .Where(x => !x.Identity.UniqueName.StartsWith(Constants.VSTFSUniqueNamePrefix))
            .ToList();
    }
}