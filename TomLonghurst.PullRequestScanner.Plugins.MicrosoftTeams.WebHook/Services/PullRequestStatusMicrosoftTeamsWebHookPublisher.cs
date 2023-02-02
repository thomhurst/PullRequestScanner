using TomLonghurst.PullRequestScanner.Contracts;
using TomLonghurst.PullRequestScanner.Enums;
using TomLonghurst.PullRequestScanner.Models;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Http;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Mappers;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Options;

namespace TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Services;

public class PullRequestStatusMicrosoftTeamsWebHookPublisher : MicrosoftTeamsWebHookPublisherBase, IPullRequestPlugin
{
    private readonly IPullRequestStatusCardMapper _pullRequestStatusCardMapper;
    private readonly MicrosoftTeamsStatusPublishOptions _options;

    internal PullRequestStatusMicrosoftTeamsWebHookPublisher(MicrosoftTeamsWebhookClient microsoftTeamsWebhookClient, IPullRequestStatusCardMapper pullRequestStatusCardMapper, MicrosoftTeamsStatusPublishOptions options) : base(microsoftTeamsWebhookClient)
    {
        _pullRequestStatusCardMapper = pullRequestStatusCardMapper;
        _options = options;
    }
    
    public override async Task ExecuteAsync(IReadOnlyList<PullRequest> pullRequests)
    {
        foreach (var pullRequestStatus in _options.StatusesToPublish)
        {
            await ExecuteAsync(pullRequests, pullRequestStatus);
        }
    }
    
    public Task ExecuteAsync(IReadOnlyList<PullRequest> pullRequests, PullRequestStatus pullRequestStatus)
    {
        return Publish(() => _pullRequestStatusCardMapper.Map(pullRequests, pullRequestStatus));
    }
}