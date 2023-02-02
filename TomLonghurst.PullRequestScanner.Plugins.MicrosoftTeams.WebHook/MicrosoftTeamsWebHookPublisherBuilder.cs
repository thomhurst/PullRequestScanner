using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TomLonghurst.PullRequestScanner.Extensions;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Http;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Mappers;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Options;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Services;

namespace TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook;

public class MicrosoftTeamsWebHookPublisherBuilder
{
    private readonly PullRequestScannerBuilder _pullRequestScannerBuilder;
    private readonly IServiceCollection _services;

    internal MicrosoftTeamsWebHookPublisherBuilder(PullRequestScannerBuilder pullRequestScannerBuilder)
    {
        _pullRequestScannerBuilder = pullRequestScannerBuilder;
        _services = pullRequestScannerBuilder.Services;
    }
    
    public MicrosoftTeamsWebHookPublisherBuilder AddOverviewCardPublisher()
    {
        _services.TryAddTransient<IPullRequestsOverviewCardMapper, PullRequestsOverviewCardMapper>();
        _services.TryAddTransient<PullRequestOverviewMicrosoftTeamsWebHookPublisher>();
        
        _services.AddHttpClient<MicrosoftTeamsWebhookClient>();

        _pullRequestScannerBuilder.AddPlugin<PullRequestOverviewMicrosoftTeamsWebHookPublisher>();
        
        return this;
    }
    
    public MicrosoftTeamsWebHookPublisherBuilder AddLeaderboardCardPublisher()
    {
        _services.TryAddTransient<IPullRequestLeaderboardCardMapper, PullRequestLeaderboardCardMapper>();
        _services.TryAddTransient<PullRequestLeaderboardMicrosoftTeamsWebHookPublisher>();

        _services.AddHttpClient<MicrosoftTeamsWebhookClient>();
        
        _pullRequestScannerBuilder.AddPlugin<PullRequestLeaderboardMicrosoftTeamsWebHookPublisher>();

        return this;
    }
    
    public MicrosoftTeamsWebHookPublisherBuilder AddStatusCardsPublisher(MicrosoftTeamsStatusPublishOptions? microsoftTeamsStatusPublishOptions = null)
    {
        _services.AddSingleton(microsoftTeamsStatusPublishOptions ?? new MicrosoftTeamsStatusPublishOptions());

        _services.TryAddTransient<IPullRequestStatusCardMapper, PullRequestStatusCardMapper>();
        _services.TryAddTransient<PullRequestStatusMicrosoftTeamsWebHookPublisher>();

        _services.AddHttpClient<MicrosoftTeamsWebhookClient>();
        
        _pullRequestScannerBuilder.AddPlugin<PullRequestStatusMicrosoftTeamsWebHookPublisher>();

        return this;
    }
}