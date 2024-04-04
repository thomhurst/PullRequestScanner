namespace TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Services;

using Enums;
using TomLonghurst.PullRequestScanner.Models;
using Http;
using Mappers;
using Models;
using Options;

internal class MicrosoftTeamsWebHookPublisher : IMicrosoftTeamsWebHookPublisher
{
    private readonly IPullRequestsOverviewCardMapper pullRequestsOverviewCardMapper;
    private readonly IPullRequestStatusCardMapper pullRequestStatusCardMapper;
    private readonly IPullRequestLeaderboardCardMapper pullRequestLeaderboardCardMapper;
    private readonly MicrosoftTeamsOptions microsoftTeamsOptions;
    private readonly MicrosoftTeamsWebhookClient microsoftTeamsWebhookClient;

    public MicrosoftTeamsWebHookPublisher(
        MicrosoftTeamsOptions microsoftTeamsOptions,
        MicrosoftTeamsWebhookClient microsoftTeamsWebhookClient,
        IPullRequestsOverviewCardMapper pullRequestsOverviewCardMapper,
        IPullRequestStatusCardMapper pullRequestStatusCardMapper,
        IPullRequestLeaderboardCardMapper pullRequestLeaderboardCardMapper)
    {
        this.microsoftTeamsOptions = microsoftTeamsOptions;
        this.microsoftTeamsWebhookClient = microsoftTeamsWebhookClient;
        this.pullRequestsOverviewCardMapper = pullRequestsOverviewCardMapper;
        this.pullRequestStatusCardMapper = pullRequestStatusCardMapper;
        this.pullRequestLeaderboardCardMapper = pullRequestLeaderboardCardMapper;
    }

    public Task ExecuteAsync(IReadOnlyList<PullRequest> pullRequests)
    {
        return ExecuteAsync(pullRequests, microsoftTeamsOptions.PublishOptions);
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
        await Publish(() => pullRequestsOverviewCardMapper.Map(pullRequests));
    }

    public async Task PublishReviewerLeaderboard(IReadOnlyList<PullRequest> pullRequests)
    {
        await Publish(() => pullRequestLeaderboardCardMapper.Map(pullRequests));
    }

    public async Task PublishStatusCard(IReadOnlyList<PullRequest> pullRequests, PullRequestStatus pullRequestStatus)
    {
        await Publish(() => pullRequestStatusCardMapper.Map(pullRequests, pullRequestStatus));
    }

    private async Task Publish(Func<IEnumerable<MicrosoftTeamsAdaptiveCard>> cardGenerator)
    {
        var cards = cardGenerator();

        foreach (var microsoftTeamsAdaptiveCard in cards)
        {
            await microsoftTeamsWebhookClient.CreateTeamsNotification(microsoftTeamsAdaptiveCard);
        }
    }
}