using TomLonghurst.PullRequestScanner.Enums;

namespace TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Options;

public class MicrosoftTeamsStatusPublishOptions
{
    public List<PullRequestStatus> StatusesToPublish { get; set; } = new()
    {
        PullRequestStatus.MergeConflicts,
        PullRequestStatus.ReadyToMerge,
        PullRequestStatus.FailingChecks,
        PullRequestStatus.NeedsReviewing,
        PullRequestStatus.Rejected
    };
}