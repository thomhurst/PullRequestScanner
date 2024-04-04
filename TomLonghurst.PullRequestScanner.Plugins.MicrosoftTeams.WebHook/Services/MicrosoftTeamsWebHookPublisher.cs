// <copyright file="MicrosoftTeamsWebHookPublisher.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Services;

using TomLonghurst.PullRequestScanner.Enums;
using TomLonghurst.PullRequestScanner.Models;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Http;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Mappers;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Models;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Options;

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
        return this.ExecuteAsync(pullRequests, this.microsoftTeamsOptions.PublishOptions);
    }

    public async Task ExecuteAsync(IReadOnlyList<PullRequest> pullRequests, MicrosoftTeamsPublishOptions microsoftTeamsPublishOptions)
    {
        if (microsoftTeamsPublishOptions.PublishPullRequestOverviewCard)
        {
            await this.PublishPullRequestsOverview(pullRequests);
        }

        foreach (var pullRequestStatus in microsoftTeamsPublishOptions.CardStatusesToPublish?.ToArray() ?? Array.Empty<PullRequestStatus>())
        {
            await this.PublishStatusCard(pullRequests, pullRequestStatus);
        }

        if (microsoftTeamsPublishOptions.PublishPullRequestReviewerLeaderboardCard)
        {
            await this.PublishReviewerLeaderboard(pullRequests);
        }
    }

    public async Task PublishPullRequestsOverview(IReadOnlyList<PullRequest> pullRequests)
    {
        await this.Publish(() => this.pullRequestsOverviewCardMapper.Map(pullRequests));
    }

    public async Task PublishReviewerLeaderboard(IReadOnlyList<PullRequest> pullRequests)
    {
        await this.Publish(() => this.pullRequestLeaderboardCardMapper.Map(pullRequests));
    }

    public async Task PublishStatusCard(IReadOnlyList<PullRequest> pullRequests, PullRequestStatus pullRequestStatus)
    {
        await this.Publish(() => this.pullRequestStatusCardMapper.Map(pullRequests, pullRequestStatus));
    }

    private async Task Publish(Func<IEnumerable<MicrosoftTeamsAdaptiveCard>> cardGenerator)
    {
        var cards = cardGenerator();

        foreach (var microsoftTeamsAdaptiveCard in cards)
        {
            await this.microsoftTeamsWebhookClient.CreateTeamsNotification(microsoftTeamsAdaptiveCard);
        }
    }
}