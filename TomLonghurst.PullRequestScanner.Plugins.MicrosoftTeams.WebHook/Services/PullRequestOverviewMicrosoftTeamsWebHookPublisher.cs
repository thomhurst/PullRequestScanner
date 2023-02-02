using TomLonghurst.PullRequestScanner.Contracts;
using TomLonghurst.PullRequestScanner.Models;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Http;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Mappers;

namespace TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Services;

public class PullRequestOverviewMicrosoftTeamsWebHookPublisher : MicrosoftTeamsWebHookPublisherBase, IPullRequestPlugin
{
    private readonly IPullRequestsOverviewCardMapper _pullRequestsOverviewCardMapper;

    internal PullRequestOverviewMicrosoftTeamsWebHookPublisher(MicrosoftTeamsWebhookClient microsoftTeamsWebhookClient, IPullRequestsOverviewCardMapper pullRequestsOverviewCardMapper) : base(microsoftTeamsWebhookClient)
    {
        _pullRequestsOverviewCardMapper = pullRequestsOverviewCardMapper;
    }
    
    public override Task ExecuteAsync(IReadOnlyList<PullRequest> pullRequests)
    {
        return Publish(() => _pullRequestsOverviewCardMapper.Map(pullRequests));
    }
}