// <copyright file="PullRequestLeaderboardMicrosoftTeamsWebHookPublisher.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Services;

using TomLonghurst.PullRequestScanner.Contracts;
using TomLonghurst.PullRequestScanner.Models;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Http;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Mappers;

public class PullRequestLeaderboardMicrosoftTeamsWebHookPublisher : MicrosoftTeamsWebHookPublisherBase, IPullRequestPlugin
{
    private readonly IPullRequestLeaderboardCardMapper pullRequestLeaderboardCardMapper;

    internal PullRequestLeaderboardMicrosoftTeamsWebHookPublisher(MicrosoftTeamsWebhookClient microsoftTeamsWebhookClient, IPullRequestLeaderboardCardMapper pullRequestLeaderboardCardMapper)
        : base(microsoftTeamsWebhookClient)
    {
        this.pullRequestLeaderboardCardMapper = pullRequestLeaderboardCardMapper;
    }

    public override Task ExecuteAsync(IReadOnlyList<PullRequest> pullRequests)
    {
        return this.Publish(() => this.pullRequestLeaderboardCardMapper.Map(pullRequests));
    }
}