using TomLonghurst.PullRequestScanner.Models.Self;
using TomLonghurst.PullRequestScanner.Models.Teams;

namespace TomLonghurst.PullRequestScanner.Mappers.TeamsCards;

internal interface IPullRequestsOverviewCardMapper
{
    IEnumerable<MicrosoftTeamsAdaptiveCard> Map(IReadOnlyList<PullRequest> pullRequests);
}