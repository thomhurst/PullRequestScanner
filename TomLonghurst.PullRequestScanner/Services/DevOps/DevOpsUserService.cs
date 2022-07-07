using TomLonghurst.EnumerableAsyncProcessor.Extensions;
using TomLonghurst.PullRequestScanner.Http;
using TomLonghurst.PullRequestScanner.Models.DevOps;
using TomLonghurst.PullRequestScanner.Options;

namespace TomLonghurst.PullRequestScanner.Services.DevOps;

internal class DevOpsUserService : IInitialize, IDevOpsUserService
{
    private readonly DevOpsHttpClient _devOpsHttpClient;
    private readonly PullRequestScannerOptions _pullRequestScannerOptions;
    private bool _initialized;
    private List<DevOpsTeamMember>? _teamMembers;

    public DevOpsUserService(DevOpsHttpClient devOpsHttpClient, PullRequestScannerOptions pullRequestScannerOptions)
    {
        _devOpsHttpClient = devOpsHttpClient;
        _pullRequestScannerOptions = pullRequestScannerOptions;
    }
    
    public async Task Initialize()
    {
        if (_initialized)
        {
            return;
        }

        if (!_pullRequestScannerOptions.AzureDevOps.IsEnabled)
        {
            _initialized = true;
            return;
        }

        var teamsInProject = await _devOpsHttpClient.Get<DevOpsTeamWrapper>(
            $"https://dev.azure.com/{_pullRequestScannerOptions.AzureDevOps.OrganizationSlug}/_apis/projects/{_pullRequestScannerOptions.AzureDevOps.ProjectSlug}/teams?api-version=7.1-preview.2");

        var membersResponses = await teamsInProject.Value
            .ToAsyncProcessorBuilder()
            .SelectAsync(x => _devOpsHttpClient.Get<DevOpsTeamMembersResponseWrapper>(
                $"https://dev.azure.com/{_pullRequestScannerOptions.AzureDevOps.OrganizationSlug}/_apis/projects/{_pullRequestScannerOptions.AzureDevOps.ProjectSlug}/teams/{x.Id}/members?api-version=7.1-preview.2"))
            .ProcessInParallel(50, TimeSpan.FromSeconds(5));

        _teamMembers = membersResponses
            .SelectMany(x => x.Value)
            .Where(x => x.Identity.DisplayName != Constants.VSTSDisplayName)
            .Where(x => !x.Identity.UniqueName.StartsWith(Constants.VSTFSUniqueNamePrefix))
            .ToList();

        _initialized = true;
    }

    public IReadOnlyList<DevOpsTeamMember> GetTeamMembers()
    {
        return _teamMembers ?? new List<DevOpsTeamMember>();
    }
}