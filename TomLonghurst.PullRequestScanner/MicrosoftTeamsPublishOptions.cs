namespace TomLonghurst.PullRequestScanner;

public class MicrosoftTeamsPublishOptions
{
    public bool PublishPullRequestStatusesCard { get; set; } = true;
    public bool PublishPullRequestReviewerLeaderboardCard { get; set; } = true;
    public bool PublishPullRequestMergeConflictsCard { get; set; } = true;
    public bool PublishPullRequestReadyToMergeCard { get; set; } = true;
}