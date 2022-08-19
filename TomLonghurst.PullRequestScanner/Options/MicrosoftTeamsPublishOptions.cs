using TomLonghurst.PullRequestScanner.Enums;

namespace TomLonghurst.PullRequestScanner.Options;

public class MicrosoftTeamsPublishOptions
{
    public bool PublishPullRequestOverviewCard { get; set; } = true;
    public bool PublishPullRequestReviewerLeaderboardCard { get; set; } = true;
    public List<PullRequestStatus> CardStatusesToPublish { get; set; } = new List<PullRequestStatus>()
    {
        PullRequestStatus.MergeConflicts,
        PullRequestStatus.ReadyToMerge,
        PullRequestStatus.FailingChecks,
        PullRequestStatus.NeedsReviewing,
        PullRequestStatus.Rejected
    };
}