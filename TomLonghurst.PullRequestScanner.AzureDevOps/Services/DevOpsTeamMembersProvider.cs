using TomLonghurst.EnumerableAsyncProcessor.Extensions;
using TomLonghurst.PullRequestScanner.AzureDevOps.Extensions;
using TomLonghurst.PullRequestScanner.AzureDevOps.Http;
using TomLonghurst.PullRequestScanner.AzureDevOps.Models;
using TomLonghurst.PullRequestScanner.AzureDevOps.Options;
using TomLonghurst.PullRequestScanner.Contracts;
using TomLonghurst.PullRequestScanner.Extensions;
using TomLonghurst.PullRequestScanner.Models;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Services;

internal class AzureDevOpsTeamMembersProvider : ITeamMembersProvider
{
    private readonly AzureDevOpsHttpClient _devOpsHttpClient;
    private readonly AzureDevOpsOptions _azureDevOpsOptions;

    public AzureDevOpsTeamMembersProvider(AzureDevOpsHttpClient azureDevOpsHttpClient, AzureDevOpsOptions azureDevOpsOptions)
    {
        _devOpsHttpClient = azureDevOpsHttpClient;
        _azureDevOpsOptions = azureDevOpsOptions;
    }

    public async Task<IEnumerable<ITeamMember>> GetTeamMembers()
    {
        if (!_azureDevOpsOptions.IsEnabled)
        {
            return Array.Empty<ITeamMember>();
        }

        List<ITeamMember> allTeamMembers = new List<ITeamMember>();

        foreach (string project in _azureDevOpsOptions.GetProjects())
        {
            var teamsInProject = await _devOpsHttpClient
                .ListTeams(project)
                .ToImmutableList();

            var membersResponses = await teamsInProject
                .ToAsyncProcessorBuilder()
                .SelectAsync(x => _devOpsHttpClient.ListTeamMembers(project, x.Id).ToImmutableList())
                .ProcessInParallel(50, TimeSpan.FromSeconds(5));

            var members = membersResponses
                .SelectMany(x => x)
                .Where(x => x.Identity.DisplayName != Constants.VstsDisplayName)
                .Where(x => !x.Identity.UniqueName.StartsWith(Constants.VstfsUniqueNamePrefix))
                .ToList();

            allTeamMembers.AddRange(members);
        }

        return allTeamMembers
            .DistinctBy(x => x.Id);
    }
}