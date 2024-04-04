using TomLonghurst.PullRequestScanner.Contracts;
using TomLonghurst.PullRequestScanner.Models;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Http;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Models;

namespace TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Services;

public abstract class MicrosoftTeamsWebHookPublisherBase : IPullRequestPlugin
{
    private readonly MicrosoftTeamsWebhookClient _microsoftTeamsWebhookClient;

    internal MicrosoftTeamsWebHookPublisherBase(
        MicrosoftTeamsWebhookClient microsoftTeamsWebhookClient
    )
    {
        _microsoftTeamsWebhookClient = microsoftTeamsWebhookClient;
    }

    internal async Task Publish(Func<IEnumerable<MicrosoftTeamsAdaptiveCard>> cardGenerator)
    {
        var cards = cardGenerator();

        foreach (var microsoftTeamsAdaptiveCard in cards)
        {
            await _microsoftTeamsWebhookClient.CreateTeamsNotification(microsoftTeamsAdaptiveCard);
        }
    }

    public abstract Task ExecuteAsync(IReadOnlyList<PullRequest> pullRequests);
}