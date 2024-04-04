namespace TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Services;

using Contracts;
using TomLonghurst.PullRequestScanner.Models;
using Http;
using Mappers;

public class PullRequestOverviewMicrosoftTeamsWebHookPublisher : MicrosoftTeamsWebHookPublisherBase, IPullRequestPlugin
{
    private readonly IPullRequestsOverviewCardMapper _pullRequestsOverviewCardMapper;

    internal PullRequestOverviewMicrosoftTeamsWebHookPublisher(MicrosoftTeamsWebhookClient microsoftTeamsWebhookClient, IPullRequestsOverviewCardMapper pullRequestsOverviewCardMapper)
        : base(microsoftTeamsWebhookClient)
    {
        _pullRequestsOverviewCardMapper = pullRequestsOverviewCardMapper;
    }

    public override Task ExecuteAsync(IReadOnlyList<PullRequest> pullRequests)
    {
        return Publish(() => _pullRequestsOverviewCardMapper.Map(pullRequests));
    }
}