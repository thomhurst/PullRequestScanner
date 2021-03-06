namespace TomLonghurst.PullRequestScanner.Options;

public class MicrosoftTeamsPublishOptions
{
    public bool PublishPullRequestStatusesCard { get; set; } = true;
    public bool PublishPullRequestReviewerLeaderboardCard { get; set; } = true;
    public bool PublishPullRequestMergeConflictsCard { get; set; } = true;
    public bool PublishPullRequestReadyToMergeCard { get; set; } = true;
    public bool PublishPullRequestFailingChecksCard { get; set; } = true;
}