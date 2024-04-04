namespace TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Services;

using TomLonghurst.PullRequestScanner.Contracts;
using TomLonghurst.PullRequestScanner.Models;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Options;

public interface IMicrosoftTeamsWebHookPublisher : IPullRequestPlugin
{
    Task ExecuteAsync(IReadOnlyList<PullRequest> pullRequests, MicrosoftTeamsPublishOptions microsoftTeamsPublishOptions);
}