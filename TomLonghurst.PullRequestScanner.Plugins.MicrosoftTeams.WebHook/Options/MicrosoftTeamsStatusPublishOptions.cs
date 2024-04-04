namespace TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Options;

using Enums;

public class MicrosoftTeamsStatusPublishOptions
{
    public List<PullRequestStatus> StatusesToPublish { get; set; } =
    [
        PullRequestStatus.MergeConflicts,
        PullRequestStatus.ReadyToMerge,
        PullRequestStatus.FailingChecks,
        PullRequestStatus.NeedsReviewing,
        PullRequestStatus.Rejected
    ];
}