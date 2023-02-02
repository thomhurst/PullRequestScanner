using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TomLonghurst.PullRequestScanner.Extensions;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Http;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Mappers;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Options;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Services;

namespace TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Extensions;

public static class PullRequestScannerBuilderExtensions
{
    [Obsolete("This adds all Microsoft Teams Webhook plugins. Please instead consider using the overload with the MicrosoftTeamsWebHookPublisherBuilder to add desired plugins one by one with better configuration")]
    public static PullRequestScannerBuilder AddMicrosoftTeamsWebHookPublisher(
        this PullRequestScannerBuilder pullRequestScannerBuilder, MicrosoftTeamsOptions microsoftTeamsOptions)
    {
        pullRequestScannerBuilder.Services.AddSingleton(microsoftTeamsOptions);
        return AddMicrosoftTeamsWebHookPublisher(pullRequestScannerBuilder);
    }

    [Obsolete("This adds all Microsoft Teams Webhook plugins. Please instead consider using the overload with the MicrosoftTeamsWebHookPublisherBuilder to add desired plugins one by one with better configuration")]
    public static PullRequestScannerBuilder AddMicrosoftTeamsWebHookPublisher(
        this PullRequestScannerBuilder pullRequestScannerBuilder, Func<IServiceProvider, MicrosoftTeamsOptions> microsoftTeamsOptionsFactory)
    {
        pullRequestScannerBuilder.Services.AddSingleton(microsoftTeamsOptionsFactory);
        return AddMicrosoftTeamsWebHookPublisher(pullRequestScannerBuilder);
    }
    
    public static PullRequestScannerBuilder AddMicrosoftTeamsWebHookPublisher(
        this PullRequestScannerBuilder pullRequestScannerBuilder, MicrosoftTeamsOptions microsoftTeamsOptions,
        Action<MicrosoftTeamsWebHookPublisherBuilder> microsoftTeamsWebHookPublisherBuilder)
    {
        pullRequestScannerBuilder.Services.AddSingleton(microsoftTeamsOptions);

        microsoftTeamsWebHookPublisherBuilder(
            new MicrosoftTeamsWebHookPublisherBuilder(pullRequestScannerBuilder)
        );

        return pullRequestScannerBuilder;
    }

    public static PullRequestScannerBuilder AddMicrosoftTeamsWebHookPublisher(
        this PullRequestScannerBuilder pullRequestScannerBuilder, Func<IServiceProvider, MicrosoftTeamsOptions> microsoftTeamsOptionsFactory,
        Action<MicrosoftTeamsWebHookPublisherBuilder> microsoftTeamsWebHookPublisherBuilder)
    {
        pullRequestScannerBuilder.Services.AddSingleton(microsoftTeamsOptionsFactory);
        
        microsoftTeamsWebHookPublisherBuilder(
            new MicrosoftTeamsWebHookPublisherBuilder(pullRequestScannerBuilder)
        );
        
        return pullRequestScannerBuilder;
    }
    
    private static PullRequestScannerBuilder AddMicrosoftTeamsWebHookPublisher(this PullRequestScannerBuilder pullRequestScannerBuilder)
    {
        pullRequestScannerBuilder.Services.TryAddTransient<IPullRequestsOverviewCardMapper, PullRequestsOverviewCardMapper>();
        pullRequestScannerBuilder.Services.TryAddTransient<IPullRequestStatusCardMapper, PullRequestStatusCardMapper>();
        pullRequestScannerBuilder.Services.TryAddTransient<IPullRequestLeaderboardCardMapper, PullRequestLeaderboardCardMapper>();
        pullRequestScannerBuilder.Services.TryAddTransient<MicrosoftTeamsWebHookPublisher>();
        
        pullRequestScannerBuilder.Services.AddHttpClient<MicrosoftTeamsWebhookClient>();

        return pullRequestScannerBuilder.AddPlugin(ActivatorUtilities.GetServiceOrCreateInstance<MicrosoftTeamsWebHookPublisher>);
    }
}