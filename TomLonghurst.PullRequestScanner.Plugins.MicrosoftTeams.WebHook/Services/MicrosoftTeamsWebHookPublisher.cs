using TomLonghurst.PullRequestScanner.Contracts;
using TomLonghurst.PullRequestScanner.Enums;
using TomLonghurst.PullRequestScanner.Models;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Http;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Mappers;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Models;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Options;
using TomLonghurst.PullRequestScanner.Services;

namespace TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Services;

internal class MicrosoftTeamsWebHookPublisher : IPullRequestPlugin
{
    private readonly IPullRequestsOverviewCardMapper _pullRequestsOverviewCardMapper;
    private readonly IPullRequestStatusCardMapper _pullRequestStatusCardMapper;
    private readonly IPullRequestLeaderboardCardMapper _pullRequestLeaderboardCardMapper;
    private readonly MicrosoftTeamsPublishOptions _microsoftTeamsPublishOptions;
    private readonly MicrosoftTeamsWebhookClient _microsoftTeamsWebhookClient;

    public MicrosoftTeamsWebHookPublisher(
        MicrosoftTeamsPublishOptions microsoftTeamsPublishOptions,
        MicrosoftTeamsWebhookClient microsoftTeamsWebhookClient,
        IPullRequestsOverviewCardMapper pullRequestsOverviewCardMapper,
        IPullRequestStatusCardMapper pullRequestStatusCardMapper,
        IPullRequestLeaderboardCardMapper pullRequestLeaderboardCardMapper
    )
    {
        _microsoftTeamsPublishOptions = microsoftTeamsPublishOptions;
        _microsoftTeamsWebhookClient = microsoftTeamsWebhookClient;
        _pullRequestsOverviewCardMapper = pullRequestsOverviewCardMapper;
        _pullRequestStatusCardMapper = pullRequestStatusCardMapper;
        _pullRequestLeaderboardCardMapper = pullRequestLeaderboardCardMapper;
    }

    public Task ExecuteAsync(IReadOnlyList<PullRequest> pullRequests)
    {
        return ExecuteAsync(pullRequests, _microsoftTeamsPublishOptions);
    }
    
    public async Task ExecuteAsync(IReadOnlyList<PullRequest> pullRequests, MicrosoftTeamsPublishOptions microsoftTeamsPublishOptions)
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

    public async Task PublishPullRequestsOverview(IReadOnlyList<PullRequest> pullRequests)
    {
        await Publish(() => _pullRequestsOverviewCardMapper.Map(pullRequests));
    }

    public async Task PublishReviewerLeaderboard(IReadOnlyList<PullRequest> pullRequests)
    {
        await Publish(() => _pullRequestLeaderboardCardMapper.Map(pullRequests));
    }

    public async Task PublishStatusCard(IReadOnlyList<PullRequest> pullRequests, PullRequestStatus pullRequestStatus)
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