namespace TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Services;

using Contracts;
using TomLonghurst.PullRequestScanner.Models;
using Http;
using Mappers;

public class PullRequestLeaderboardMicrosoftTeamsWebHookPublisher : MicrosoftTeamsWebHookPublisherBase, IPullRequestPlugin
{
    private readonly IPullRequestLeaderboardCardMapper _pullRequestLeaderboardCardMapper;

    internal PullRequestLeaderboardMicrosoftTeamsWebHookPublisher(MicrosoftTeamsWebhookClient microsoftTeamsWebhookClient, IPullRequestLeaderboardCardMapper pullRequestLeaderboardCardMapper)
        : base(microsoftTeamsWebhookClient)
    {
        _pullRequestLeaderboardCardMapper = pullRequestLeaderboardCardMapper;
    }

    public override Task ExecuteAsync(IReadOnlyList<PullRequest> pullRequests)
    {
        return Publish(() => _pullRequestLeaderboardCardMapper.Map(pullRequests));
    }
}