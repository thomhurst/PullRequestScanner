﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Http;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Mappers;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Options;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Services;

namespace TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook;

public class MicrosoftTeamsWebHookPublisherBuilder
{
    private readonly IServiceCollection _services;

    internal MicrosoftTeamsWebHookPublisherBuilder(IServiceCollection services)
    {
        _services = services;
    }
    
    public MicrosoftTeamsWebHookPublisherBuilder AddOverviewCardPublisher()
    {
        _services.TryAddTransient<IPullRequestsOverviewCardMapper, PullRequestsOverviewCardMapper>();
        _services.TryAddTransient<PullRequestOverviewMicrosoftTeamsWebHookPublisher>();
        
        _services.AddHttpClient<MicrosoftTeamsWebhookClient>();

        return this;
    }
    
    public MicrosoftTeamsWebHookPublisherBuilder AddLeaderboardCardPublisher()
    {
        _services.TryAddTransient<IPullRequestLeaderboardCardMapper, PullRequestLeaderboardCardMapper>();
        _services.TryAddTransient<PullRequestLeaderboardMicrosoftTeamsWebHookPublisher>();
        
        _services.AddHttpClient<MicrosoftTeamsWebhookClient>();

        return this;
    }
    
    public MicrosoftTeamsWebHookPublisherBuilder AddStatusCardsPublisher(MicrosoftTeamsStatusPublishOptions? microsoftTeamsStatusPublishOptions = null)
    {
        _services.AddSingleton(microsoftTeamsStatusPublishOptions ?? new MicrosoftTeamsStatusPublishOptions());

        _services.TryAddTransient<IPullRequestStatusCardMapper, PullRequestStatusCardMapper>();
        _services.TryAddTransient<PullRequestStatusMicrosoftTeamsWebHookPublisher>();
        
        _services.AddHttpClient<MicrosoftTeamsWebhookClient>();

        return this;
    }
}