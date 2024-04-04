namespace TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Mappers;

using TomLonghurst.PullRequestScanner.Models;
using Models;

internal interface IPullRequestsOverviewCardMapper
{
    IEnumerable<MicrosoftTeamsAdaptiveCard> Map(IReadOnlyList<PullRequest> pullRequests);
}