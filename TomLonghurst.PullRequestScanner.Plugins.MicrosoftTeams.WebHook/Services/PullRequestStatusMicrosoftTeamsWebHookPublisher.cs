namespace TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Services;

using TomLonghurst.PullRequestScanner.Contracts;
using TomLonghurst.PullRequestScanner.Enums;
using TomLonghurst.PullRequestScanner.Models;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Http;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Mappers;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Options;

public class PullRequestStatusMicrosoftTeamsWebHookPublisher : MicrosoftTeamsWebHookPublisherBase, IPullRequestPlugin
{
    private readonly IPullRequestStatusCardMapper pullRequestStatusCardMapper;
    private readonly MicrosoftTeamsStatusPublishOptions options;

    internal PullRequestStatusMicrosoftTeamsWebHookPublisher(MicrosoftTeamsWebhookClient microsoftTeamsWebhookClient, IPullRequestStatusCardMapper pullRequestStatusCardMapper, MicrosoftTeamsStatusPublishOptions options)
        : base(microsoftTeamsWebhookClient)
    {
        this.pullRequestStatusCardMapper = pullRequestStatusCardMapper;
        this.options = options;
    }

    public override async Task ExecuteAsync(IReadOnlyList<PullRequest> pullRequests)
    {
        foreach (var pullRequestStatus in options.StatusesToPublish)
        {
            await ExecuteAsync(pullRequests, pullRequestStatus);
        }
    }

    public Task ExecuteAsync(IReadOnlyList<PullRequest> pullRequests, PullRequestStatus pullRequestStatus)
    {
        return Publish(() => pullRequestStatusCardMapper.Map(pullRequests, pullRequestStatus));
    }
}