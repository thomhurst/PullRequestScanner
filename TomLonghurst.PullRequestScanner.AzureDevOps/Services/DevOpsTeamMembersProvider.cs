using EnumerableAsyncProcessor.Extensions;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using TomLonghurst.PullRequestScanner.AzureDevOps.Options;
using TomLonghurst.PullRequestScanner.Contracts;
using TomLonghurst.PullRequestScanner.Models;
using TeamMember = Microsoft.VisualStudio.Services.WebApi.TeamMember;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Services;

internal class AzureDevOpsTeamMembersProvider : ITeamMembersProvider
{
    private readonly AzureDevOpsOptions _azureDevOpsOptions;
    private readonly VssConnection _vssConnection;

    public AzureDevOpsTeamMembersProvider(AzureDevOpsOptions azureDevOpsOptions, VssConnection vssConnection)
    {
        _azureDevOpsOptions = azureDevOpsOptions;
        _vssConnection = vssConnection;
    }

    public async Task<IEnumerable<ITeamMember>> GetTeamMembers()
    {
        if (!_azureDevOpsOptions.IsEnabled)
        {
            return Array.Empty<ITeamMember>();
        }

        var teams = new List<WebApiTeam>();

        var iteration = 0;
        do
        {
            var teamsInIteration = await _vssConnection.GetClient<TeamHttpClient>().GetTeamsAsync(
                projectId: _azureDevOpsOptions.ProjectGuid.ToString(),
                top: 100,
                skip: 100 * iteration);

            teams.AddRange(teamsInIteration);

            iteration++;
        } while (teams.Count == 100 * iteration);

        var membersResponsesArrays = await teams
            .ToAsyncProcessorBuilder()
            .SelectAsync(GetTeamMembers)
            .ProcessInParallel(50, TimeSpan.FromSeconds(5));

        var membersResponses = membersResponsesArrays
            .SelectMany(x => x)
            .ToList();

        return membersResponses
            .Where(x => x.Identity.DisplayName != Constants.VstsDisplayName)
            .Where(x => !x.Identity.UniqueName.StartsWith(Constants.VstfsUniqueNamePrefix))
            .Select(x => new TeamMemberImpl
            {
                DisplayName = x.Identity.DisplayName,
                UniqueName = x.Identity.UniqueName,
                Email = x.Identity.UniqueName,
                Id = x.Identity.Id
            })
            .ToList();
    }

    private async Task<List<TeamMember>> GetTeamMembers(WebApiTeam team)
    {
        var members = new List<TeamMember>();

        var iteration = 0;
        do
        {
            var membersInIteration = await _vssConnection.GetClient<TeamHttpClient>().GetTeamMembersWithExtendedPropertiesAsync(
                projectId: _azureDevOpsOptions.ProjectGuid.ToString(),
                teamId: team.Id.ToString(),
                top: 100,
                skip: 100 * iteration);

            members.AddRange(membersInIteration);

            iteration++;
        } while (members.Count == 100 * iteration);

        return members;
    }
}