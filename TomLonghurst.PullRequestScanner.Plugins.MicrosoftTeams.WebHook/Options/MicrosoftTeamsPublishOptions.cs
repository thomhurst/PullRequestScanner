namespace TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Options;

using Enums;

public class MicrosoftTeamsPublishOptions
{
    public bool PublishPullRequestOverviewCard { get; set; } = true;

    public bool PublishPullRequestReviewerLeaderboardCard { get; set; } = true;

    public List<PullRequestStatus> CardStatusesToPublish { get; set; } =
    [
        PullRequestStatus.MergeConflicts,
        PullRequestStatus.ReadyToMerge,
        PullRequestStatus.FailingChecks,
        PullRequestStatus.NeedsReviewing,
        PullRequestStatus.Rejected
    ];
}