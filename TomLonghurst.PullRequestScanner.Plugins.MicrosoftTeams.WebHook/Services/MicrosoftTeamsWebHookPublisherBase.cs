namespace TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Services;

using TomLonghurst.PullRequestScanner.Contracts;
using TomLonghurst.PullRequestScanner.Models;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Http;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Models;

public abstract class MicrosoftTeamsWebHookPublisherBase : IPullRequestPlugin
{
    private readonly MicrosoftTeamsWebhookClient microsoftTeamsWebhookClient;

    internal MicrosoftTeamsWebHookPublisherBase(
        MicrosoftTeamsWebhookClient microsoftTeamsWebhookClient)
    {
        this.microsoftTeamsWebhookClient = microsoftTeamsWebhookClient;
    }

    internal async Task Publish(Func<IEnumerable<MicrosoftTeamsAdaptiveCard>> cardGenerator)
    {
        var cards = cardGenerator();

        foreach (var microsoftTeamsAdaptiveCard in cards)
        {
            await microsoftTeamsWebhookClient.CreateTeamsNotification(microsoftTeamsAdaptiveCard);
        }
    }

    public abstract Task ExecuteAsync(IReadOnlyList<PullRequest> pullRequests);
}