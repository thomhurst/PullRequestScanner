namespace TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Services;

using TomLonghurst.PullRequestScanner.Contracts;
using TomLonghurst.PullRequestScanner.Models;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Http;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Mappers;

public class PullRequestLeaderboardMicrosoftTeamsWebHookPublisher : MicrosoftTeamsWebHookPublisherBase, IPullRequestPlugin
{
    private readonly IPullRequestLeaderboardCardMapper _pullRequestLeaderboardCardMapper;

    internal PullRequestLeaderboardMicrosoftTeamsWebHookPublisher(MicrosoftTeamsWebhookClient microsoftTeamsWebhookClient, IPullRequestLeaderboardCardMapper pullRequestLeaderboardCardMapper)
        : base(microsoftTeamsWebhookClient)
    {
        this._pullRequestLeaderboardCardMapper = pullRequestLeaderboardCardMapper;
    }

    public override Task ExecuteAsync(IReadOnlyList<PullRequest> pullRequests)
    {
        return Publish(() => _pullRequestLeaderboardCardMapper.Map(pullRequests));
    }
}