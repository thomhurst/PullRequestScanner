namespace TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Services;

using Contracts;
using TomLonghurst.PullRequestScanner.Models;
using Http;
using Models;

public abstract class MicrosoftTeamsWebHookPublisherBase : IPullRequestPlugin
{
    private readonly MicrosoftTeamsWebhookClient _microsoftTeamsWebhookClient;

    internal MicrosoftTeamsWebHookPublisherBase(
        MicrosoftTeamsWebhookClient microsoftTeamsWebhookClient)
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