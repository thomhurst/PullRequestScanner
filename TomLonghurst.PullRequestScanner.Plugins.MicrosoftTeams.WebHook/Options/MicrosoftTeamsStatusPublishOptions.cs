namespace TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Options;

using TomLonghurst.PullRequestScanner.Enums;

public class MicrosoftTeamsStatusPublishOptions
{
    public List<PullRequestStatus> StatusesToPublish { get; set; } = new()
    {
        PullRequestStatus.MergeConflicts,
        PullRequestStatus.ReadyToMerge,
        PullRequestStatus.FailingChecks,
        PullRequestStatus.NeedsReviewing,
        PullRequestStatus.Rejected,
    };
}