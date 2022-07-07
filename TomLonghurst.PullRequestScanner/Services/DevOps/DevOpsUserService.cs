using TomLonghurst.EnumerableAsyncProcessor.Extensions;
using TomLonghurst.PullRequestScanner.Http;
using TomLonghurst.PullRequestScanner.Models.DevOps;
using TomLonghurst.PullRequestScanner.Options;

namespace TomLonghurst.PullRequestScanner.Services.DevOps;

internal class DevOpsUserService : IDevOpsUserService
{
    private readonly DevOpsHttpClient _devOpsHttpClient;
    private readonly PullRequestScannerOptions _pullRequestScannerOptions;

    public DevOpsUserService(DevOpsHttpClient devOpsHttpClient, PullRequestScannerOptions pullRequestScannerOptions)
    {
        _devOpsHttpClient = devOpsHttpClient;
        _pullRequestScannerOptions = pullRequestScannerOptions;
    }

    public async Task<IReadOnlyList<DevOpsTeamMember>> GetTeamMembers()
    {
        if (!_pullRequestScannerOptions.AzureDevOps.IsEnabled)
        {
            return new List<DevOpsTeamMember>();
        }

        var teamsInProject = await _devOpsHttpClient.Get<DevOpsTeamWrapper>(
            $"https://dev.azure.com/{_pullRequestScannerOptions.AzureDevOps.OrganizationSlug}/_apis/projects/{_pullRequestScannerOptions.AzureDevOps.ProjectSlug}/teams?api-version=7.1-preview.2");

        var membersResponses = await teamsInProject.Value
            .ToAsyncProcessorBuilder()
            .SelectAsync(x => _devOpsHttpClient.Get<DevOpsTeamMembersResponseWrapper>(
                $"https://dev.azure.com/{_pullRequestScannerOptions.AzureDevOps.OrganizationSlug}/_apis/projects/{_pullRequestScannerOptions.AzureDevOps.ProjectSlug}/teams/{x.Id}/members?api-version=7.1-preview.2"))
            .ProcessInParallel(50, TimeSpan.FromSeconds(5));

        return membersResponses
            .SelectMany(x => x.Value)
            .Where(x => x.Identity.DisplayName != Constants.VSTSDisplayName)
            .Where(x => !x.Identity.UniqueName.StartsWith(Constants.VSTFSUniqueNamePrefix))
            .ToList();
    }
}