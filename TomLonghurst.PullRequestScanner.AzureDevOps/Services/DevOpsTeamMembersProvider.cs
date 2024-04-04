namespace TomLonghurst.PullRequestScanner.AzureDevOps.Services;

using EnumerableAsyncProcessor.Extensions;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using TomLonghurst.PullRequestScanner.AzureDevOps.Options;
using TomLonghurst.PullRequestScanner.Contracts;
using TomLonghurst.PullRequestScanner.Models;
using TeamMember = Microsoft.VisualStudio.Services.WebApi.TeamMember;

internal class AzureDevOpsTeamMembersProvider(AzureDevOpsOptions azureDevOpsOptions, VssConnection vssConnection) : ITeamMembersProvider
{
    public async Task<IEnumerable<ITeamMember>> GetTeamMembers()
    {
        if (!azureDevOpsOptions.IsEnabled)
        {
            return [];
        }

        var teams = new List<WebApiTeam>();

        var iteration = 0;
        do
        {
            var teamsInIteration = await vssConnection.GetClient<TeamHttpClient>().GetTeamsAsync(
                projectId: azureDevOpsOptions.ProjectGuid.ToString(),
                top: 100,
                skip: 100 * iteration);

            teams.AddRange(teamsInIteration);

            iteration++;
        }
        while (teams.Count == 100 * iteration);

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
                Id = x.Identity.Id,
            })
            .ToList();
    }

    private async Task<List<TeamMember>> GetTeamMembers(WebApiTeam team)
    {
        var members = new List<TeamMember>();

        var iteration = 0;
        do
        {
            var membersInIteration = await vssConnection.GetClient<TeamHttpClient>().GetTeamMembersWithExtendedPropertiesAsync(
                projectId: azureDevOpsOptions.ProjectGuid.ToString(),
                teamId: team.Id.ToString(),
                top: 100,
                skip: 100 * iteration);

            members.AddRange(membersInIteration);

            iteration++;
        }
        while (members.Count == 100 * iteration);

        return members;
    }
}