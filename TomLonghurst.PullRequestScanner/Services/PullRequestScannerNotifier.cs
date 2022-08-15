using System.Collections.Concurrent;
using AdaptiveCards;
using TomLonghurst.PullRequestScanner.Enums;
using TomLonghurst.PullRequestScanner.Extensions;
using TomLonghurst.PullRequestScanner.Http;
using TomLonghurst.PullRequestScanner.Mappers;
using TomLonghurst.PullRequestScanner.Mappers.TeamsCards;
using TomLonghurst.PullRequestScanner.Models.Self;
using TomLonghurst.PullRequestScanner.Models.Teams;
using TomLonghurst.PullRequestScanner.Options;

namespace TomLonghurst.PullRequestScanner.Services;

internal class PullRequestScannerNotifier : IPullRequestScannerNotifier
{
    public PullRequestScannerNotifier(MicrosoftTeamsWebhookClient microsoftTeamsWebhookClient,
        IPullRequestService pullRequestService,
        IPullRequestStatusesCardMapper pullRequestStatusesCardMapper,
        IPullRequestStatusCardMapper pullRequestStatusCardMapper,
        IPullRequestLeaderboardCardMapper pullRequestLeaderboardCardMapper)
    {
        _microsoftTeamsWebhookClient = microsoftTeamsWebhookClient;
        _pullRequestService = pullRequestService;
        _pullRequestStatusesCardMapper = pullRequestStatusesCardMapper;
        _pullRequestStatusCardMapper = pullRequestStatusCardMapper;
        _pullRequestLeaderboardCardMapper = pullRequestLeaderboardCardMapper;
    }

    private readonly IPullRequestService _pullRequestService;
    private readonly IPullRequestStatusesCardMapper _pullRequestStatusesCardMapper;
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
        if (microsoftTeamsPublishOptions.PublishPullRequestStatusesCard)
        {
            await PublishPullRequestStatuses(pullRequests);
        }
        
        if (microsoftTeamsPublishOptions.PublishPullRequestMergeConflictsCard)
        {
            await PublishStatusCard(pullRequests, PullRequestStatus.MergeConflicts);
        }
        
        if (microsoftTeamsPublishOptions.PublishPullRequestReadyToMergeCard)
        {
            await PublishStatusCard(pullRequests, PullRequestStatus.ReadyToMerge);
        }
        
        if (microsoftTeamsPublishOptions.PublishPullRequestFailingChecksCard)
        {
            await PublishStatusCard(pullRequests, PullRequestStatus.FailingChecks);
        }
        
        if (microsoftTeamsPublishOptions.PublishPullRequestReviewerLeaderboardCard)
        {
            await PublishReviewerLeaderboard(pullRequests);
        }
    }

    private async Task PublishPullRequestStatuses(IReadOnlyList<PullRequest> pullRequests)
    {
        await Publish(() => _pullRequestStatusesCardMapper.Map(pullRequests));
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