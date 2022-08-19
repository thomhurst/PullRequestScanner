using TomLonghurst.PullRequestScanner.Enums;
using TomLonghurst.PullRequestScanner.Http;
using TomLonghurst.PullRequestScanner.Mappers.TeamsCards;
using TomLonghurst.PullRequestScanner.Models.Self;
using TomLonghurst.PullRequestScanner.Models.Teams;
using TomLonghurst.PullRequestScanner.Options;

namespace TomLonghurst.PullRequestScanner.Services;

internal class PullRequestScannerNotifier : IPullRequestScannerNotifier
{
    public PullRequestScannerNotifier(MicrosoftTeamsWebhookClient microsoftTeamsWebhookClient,
        IPullRequestService pullRequestService,
        IPullRequestsOverviewCardMapper pullRequestsOverviewCardMapper,
        IPullRequestStatusCardMapper pullRequestStatusCardMapper,
        IPullRequestLeaderboardCardMapper pullRequestLeaderboardCardMapper)
    {
        _microsoftTeamsWebhookClient = microsoftTeamsWebhookClient;
        _pullRequestService = pullRequestService;
        _pullRequestsOverviewCardMapper = pullRequestsOverviewCardMapper;
        _pullRequestStatusCardMapper = pullRequestStatusCardMapper;
        _pullRequestLeaderboardCardMapper = pullRequestLeaderboardCardMapper;
    }

    private readonly IPullRequestService _pullRequestService;
    private readonly IPullRequestsOverviewCardMapper _pullRequestsOverviewCardMapper;
    private readonly IPullRequestStatusCardMapper _pullRequestStatusCardMapper;
    private readonly IPullRequestLeaderboardCardMapper _pullRequestLeaderboardCardMapper;
    private readonly MicrosoftTeamsWebhookClient _microsoftTeamsWebhookClient;

    public async Task NotifyTeamsChannel(MicrosoftTeamsPublishOptions microsoftTeamsPublishOptions)
    {
        var pullRequests = await _pullRequestService.GetPullRequests();
        await NotifyTeamsChannel(pullRequests, microsoftTeamsPublishOptions);
    }

    public async Task NotifyTeamsChannel(IReadOnlyList<PullRequest> pullRequests, MicrosoftTeamsPublishOptions microsoftTeamsPublishOptions)
    {
        if (microsoftTeamsPublishOptions.PublishPullRequestOverviewCard)
        {
            await PublishPullRequestsOverview(pullRequests);
        }
        
        foreach (var pullRequestStatus in microsoftTeamsPublishOptions.CardStatusesToPublish?.ToArray() ?? Array.Empty<PullRequestStatus>())
        {
            await PublishStatusCard(pullRequests, pullRequestStatus);
        }

        if (microsoftTeamsPublishOptions.PublishPullRequestReviewerLeaderboardCard)
        {
            await PublishReviewerLeaderboard(pullRequests);
        }
    }

    private async Task PublishPullRequestsOverview(IReadOnlyList<PullRequest> pullRequests)
    {
        await Publish(() => _pullRequestsOverviewCardMapper.Map(pullRequests));
    }

    private async Task PublishReviewerLeaderboard(IReadOnlyList<PullRequest> pullRequests)
    {
        await Publish(() => _pullRequestLeaderboardCardMapper.Map(pullRequests));
    }

    private async Task PublishStatusCard(IReadOnlyList<PullRequest> pullRequests, PullRequestStatus pullRequestStatus)
    {
        await Publish(() => _pullRequestStatusCardMapper.Map(pullRequests, pullRequestStatus));
    }

    private async Task Publish(Func<IEnumerable<MicrosoftTeamsAdaptiveCard>> cardGenerator)
    {
        var cards = cardGenerator();
        
        foreach (var microsoftTeamsAdaptiveCard in cards)
        {
            await _microsoftTeamsWebhookClient.CreateTeamsNotification(microsoftTeamsAdaptiveCard);
        }
    }
}