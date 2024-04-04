namespace TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TomLonghurst.PullRequestScanner.Extensions;
using Http;
using Mappers;
using Options;
using Services;

public class MicrosoftTeamsWebHookPublisherBuilder
{
    private readonly PullRequestScannerBuilder pullRequestScannerBuilder;
    private readonly IServiceCollection services;

    internal MicrosoftTeamsWebHookPublisherBuilder(PullRequestScannerBuilder pullRequestScannerBuilder)
    {
        this.pullRequestScannerBuilder = pullRequestScannerBuilder;
        services = pullRequestScannerBuilder.Services;
    }

    public MicrosoftTeamsWebHookPublisherBuilder AddOverviewCardPublisher()
    {
        services.TryAddTransient<IPullRequestsOverviewCardMapper, PullRequestsOverviewCardMapper>();
        services.TryAddTransient<PullRequestOverviewMicrosoftTeamsWebHookPublisher>(sp =>
            new PullRequestOverviewMicrosoftTeamsWebHookPublisher(sp.GetRequiredService<MicrosoftTeamsWebhookClient>(), sp.GetRequiredService<IPullRequestsOverviewCardMapper>()));

        services.AddHttpClient<MicrosoftTeamsWebhookClient>();

        pullRequestScannerBuilder.AddPlugin(sp => sp.GetRequiredService<PullRequestOverviewMicrosoftTeamsWebHookPublisher>());

        return this;
    }

    public MicrosoftTeamsWebHookPublisherBuilder AddLeaderboardCardPublisher()
    {
        services.TryAddTransient<IPullRequestLeaderboardCardMapper, PullRequestLeaderboardCardMapper>();
        services.TryAddTransient<PullRequestLeaderboardMicrosoftTeamsWebHookPublisher>(sp =>
            new PullRequestLeaderboardMicrosoftTeamsWebHookPublisher(sp.GetRequiredService<MicrosoftTeamsWebhookClient>(), sp.GetRequiredService<IPullRequestLeaderboardCardMapper>()));

        services.AddHttpClient<MicrosoftTeamsWebhookClient>();

        pullRequestScannerBuilder.AddPlugin(sp => sp.GetRequiredService<PullRequestLeaderboardMicrosoftTeamsWebHookPublisher>());

        return this;
    }

    public MicrosoftTeamsWebHookPublisherBuilder AddStatusCardsPublisher(MicrosoftTeamsStatusPublishOptions? microsoftTeamsStatusPublishOptions = null)
    {
        services.AddSingleton(microsoftTeamsStatusPublishOptions ?? new MicrosoftTeamsStatusPublishOptions());

        services.TryAddTransient<IPullRequestStatusCardMapper, PullRequestStatusCardMapper>();
        services.TryAddTransient<PullRequestStatusMicrosoftTeamsWebHookPublisher>(sp =>
            new PullRequestStatusMicrosoftTeamsWebHookPublisher(sp.GetRequiredService<MicrosoftTeamsWebhookClient>(), sp.GetRequiredService<IPullRequestStatusCardMapper>(), microsoftTeamsStatusPublishOptions ?? new MicrosoftTeamsStatusPublishOptions()));

        services.AddHttpClient<MicrosoftTeamsWebhookClient>();

        pullRequestScannerBuilder.AddPlugin(sp => sp.GetRequiredService<PullRequestStatusMicrosoftTeamsWebHookPublisher>());

        return this;
    }
}