using Microsoft.Extensions.DependencyInjection;
using TomLonghurst.PullRequestScanner.Extensions;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Http;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Mappers;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Options;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Services;

namespace TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Extensions;

public static class PullRequestScannerBuilderExtensions
{
    public static PullRequestScannerBuilder AddMicrosoftTeamsWebHookPublisher(
        this PullRequestScannerBuilder pullRequestScannerBuilder, MicrosoftTeamsOptions microsoftTeamsOptions)
    {
        pullRequestScannerBuilder.Services.AddSingleton(microsoftTeamsOptions);
        return AddMicrosoftTeamsWebHookPublisher(pullRequestScannerBuilder);
    }

    public static PullRequestScannerBuilder AddMicrosoftTeamsWebHookPublisher(
        this PullRequestScannerBuilder pullRequestScannerBuilder, Func<IServiceProvider, MicrosoftTeamsOptions> microsoftTeamsOptionsFactory)
    {
        pullRequestScannerBuilder.Services.AddSingleton(microsoftTeamsOptionsFactory);
        return AddMicrosoftTeamsWebHookPublisher(pullRequestScannerBuilder);
    }

    private static PullRequestScannerBuilder AddMicrosoftTeamsWebHookPublisher(this PullRequestScannerBuilder pullRequestScannerBuilder)
    {
        pullRequestScannerBuilder.Services
            .AddTransient<IPullRequestsOverviewCardMapper, PullRequestsOverviewCardMapper>()
            .AddTransient<IPullRequestStatusCardMapper, PullRequestStatusCardMapper>()
            .AddTransient<IPullRequestLeaderboardCardMapper, PullRequestLeaderboardCardMapper>()
            .AddTransient<MicrosoftTeamsWebHookPublisher>()
            .AddHttpClient<MicrosoftTeamsWebhookClient>();;

        return pullRequestScannerBuilder.AddPlugin(ActivatorUtilities.GetServiceOrCreateInstance<MicrosoftTeamsWebHookPublisher>);
    }
}