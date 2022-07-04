using TomLonghurst.PullRequestScanner.Models.Self;
using TomLonghurst.PullRequestScanner.Options;

namespace TomLonghurst.PullRequestScanner.Services;

public interface IPullRequestScannerNotifier
{
    Task NotifyTeamsChannel(MicrosoftTeamsPublishOptions microsoftTeamsPublishOptions);
    Task NotifyTeamsChannel(IReadOnlyList<PullRequest> pullRequests, MicrosoftTeamsPublishOptions microsoftTeamsPublishOptions);
}