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
        _services.TryAddTransient<PullRequestOverviewMicrosoftTeamsWebHookPublisher>(sp => 
            new PullRequestOverviewMicrosoftTeamsWebHookPublisher(sp.GetRequiredService<MicrosoftTeamsWebhookClient>(), sp.GetRequiredService<IPullRequestsOverviewCardMapper>())
        );
        
        _services.AddHttpClient<MicrosoftTeamsWebhookClient>();
        
        _pullRequestScannerBuilder.AddPlugin(sp => sp.GetRequiredService<PullRequestOverviewMicrosoftTeamsWebHookPublisher>());
        
        return this;
    }
    
    public MicrosoftTeamsWebHookPublisherBuilder AddLeaderboardCardPublisher()
    {
        _services.TryAddTransient<IPullRequestLeaderboardCardMapper, PullRequestLeaderboardCardMapper>();
        _services.TryAddTransient<PullRequestLeaderboardMicrosoftTeamsWebHookPublisher>(sp => 
            new PullRequestLeaderboardMicrosoftTeamsWebHookPublisher(sp.GetRequiredService<MicrosoftTeamsWebhookClient>(), sp.GetRequiredService<IPullRequestLeaderboardCardMapper>())
        );

        _services.AddHttpClient<MicrosoftTeamsWebhookClient>();
        
        _pullRequestScannerBuilder.AddPlugin(sp => sp.GetRequiredService<PullRequestLeaderboardMicrosoftTeamsWebHookPublisher>());

        return this;
    }
    
    public MicrosoftTeamsWebHookPublisherBuilder AddStatusCardsPublisher(MicrosoftTeamsStatusPublishOptions? microsoftTeamsStatusPublishOptions = null)
    {
        _services.AddSingleton(microsoftTeamsStatusPublishOptions ?? new MicrosoftTeamsStatusPublishOptions());

        _services.TryAddTransient<IPullRequestStatusCardMapper, PullRequestStatusCardMapper>();
        _services.TryAddTransient<PullRequestStatusMicrosoftTeamsWebHookPublisher>(sp => 
            new PullRequestStatusMicrosoftTeamsWebHookPublisher(sp.GetRequiredService<MicrosoftTeamsWebhookClient>(), sp.GetRequiredService<IPullRequestStatusCardMapper>(), microsoftTeamsStatusPublishOptions ?? new MicrosoftTeamsStatusPublishOptions())
        );
        
        _services.AddHttpClient<MicrosoftTeamsWebhookClient>();
        
        _pullRequestScannerBuilder.AddPlugin(sp => sp.GetRequiredService<PullRequestStatusMicrosoftTeamsWebHookPublisher>());

        return this;
    }
}