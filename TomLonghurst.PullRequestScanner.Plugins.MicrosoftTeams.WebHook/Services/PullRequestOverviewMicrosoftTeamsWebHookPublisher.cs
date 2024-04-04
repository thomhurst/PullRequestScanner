namespace TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Services;

using TomLonghurst.PullRequestScanner.Contracts;
using TomLonghurst.PullRequestScanner.Models;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Http;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Mappers;

public class PullRequestOverviewMicrosoftTeamsWebHookPublisher : MicrosoftTeamsWebHookPublisherBase, IPullRequestPlugin
{
    private readonly IPullRequestsOverviewCardMapper pullRequestsOverviewCardMapper;

    internal PullRequestOverviewMicrosoftTeamsWebHookPublisher(MicrosoftTeamsWebhookClient microsoftTeamsWebhookClient, IPullRequestsOverviewCardMapper pullRequestsOverviewCardMapper)
        : base(microsoftTeamsWebhookClient)
    {
        this.pullRequestsOverviewCardMapper = pullRequestsOverviewCardMapper;
    }

    public override Task ExecuteAsync(IReadOnlyList<PullRequest> pullRequests)
    {
        return Publish(() => pullRequestsOverviewCardMapper.Map(pullRequests));
    }
}