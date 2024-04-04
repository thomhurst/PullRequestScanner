// <copyright file="MicrosoftTeamsWebHookPublisherBuilder.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TomLonghurst.PullRequestScanner.Extensions;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Http;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Mappers;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Options;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Services;

public class MicrosoftTeamsWebHookPublisherBuilder
{
    private readonly PullRequestScannerBuilder pullRequestScannerBuilder;
    private readonly IServiceCollection services;

    internal MicrosoftTeamsWebHookPublisherBuilder(PullRequestScannerBuilder pullRequestScannerBuilder)
    {
        this.pullRequestScannerBuilder = pullRequestScannerBuilder;
        this.services = pullRequestScannerBuilder.Services;
    }

    public MicrosoftTeamsWebHookPublisherBuilder AddOverviewCardPublisher()
    {
        this.services.TryAddTransient<IPullRequestsOverviewCardMapper, PullRequestsOverviewCardMapper>();
        this.services.TryAddTransient<PullRequestOverviewMicrosoftTeamsWebHookPublisher>(sp =>
            new PullRequestOverviewMicrosoftTeamsWebHookPublisher(sp.GetRequiredService<MicrosoftTeamsWebhookClient>(), sp.GetRequiredService<IPullRequestsOverviewCardMapper>()));

        this.services.AddHttpClient<MicrosoftTeamsWebhookClient>();

        this.pullRequestScannerBuilder.AddPlugin(sp => sp.GetRequiredService<PullRequestOverviewMicrosoftTeamsWebHookPublisher>());

        return this;
    }

    public MicrosoftTeamsWebHookPublisherBuilder AddLeaderboardCardPublisher()
    {
        this.services.TryAddTransient<IPullRequestLeaderboardCardMapper, PullRequestLeaderboardCardMapper>();
        this.services.TryAddTransient<PullRequestLeaderboardMicrosoftTeamsWebHookPublisher>(sp =>
            new PullRequestLeaderboardMicrosoftTeamsWebHookPublisher(sp.GetRequiredService<MicrosoftTeamsWebhookClient>(), sp.GetRequiredService<IPullRequestLeaderboardCardMapper>()));

        this.services.AddHttpClient<MicrosoftTeamsWebhookClient>();

        this.pullRequestScannerBuilder.AddPlugin(sp => sp.GetRequiredService<PullRequestLeaderboardMicrosoftTeamsWebHookPublisher>());

        return this;
    }

    public MicrosoftTeamsWebHookPublisherBuilder AddStatusCardsPublisher(MicrosoftTeamsStatusPublishOptions? microsoftTeamsStatusPublishOptions = null)
    {
        this.services.AddSingleton(microsoftTeamsStatusPublishOptions ?? new MicrosoftTeamsStatusPublishOptions());

        this.services.TryAddTransient<IPullRequestStatusCardMapper, PullRequestStatusCardMapper>();
        this.services.TryAddTransient<PullRequestStatusMicrosoftTeamsWebHookPublisher>(sp =>
            new PullRequestStatusMicrosoftTeamsWebHookPublisher(sp.GetRequiredService<MicrosoftTeamsWebhookClient>(), sp.GetRequiredService<IPullRequestStatusCardMapper>(), microsoftTeamsStatusPublishOptions ?? new MicrosoftTeamsStatusPublishOptions()));

        this.services.AddHttpClient<MicrosoftTeamsWebhookClient>();

        this.pullRequestScannerBuilder.AddPlugin(sp => sp.GetRequiredService<PullRequestStatusMicrosoftTeamsWebHookPublisher>());

        return this;
    }
}