using TomLonghurst.PullRequestScanner.Enums;
using TomLonghurst.PullRequestScanner.Models.Self;
using TomLonghurst.PullRequestScanner.Models.Teams;

namespace TomLonghurst.PullRequestScanner.Mappers.TeamsCards;

internal interface IPullRequestStatusCardMapper
{
    IEnumerable<MicrosoftTeamsAdaptiveCard> Map(IReadOnlyList<PullRequest> pullRequests, PullRequestStatus pullRequestStatus);
}