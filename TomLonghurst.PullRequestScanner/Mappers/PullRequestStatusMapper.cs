namespace TomLonghurst.PullRequestScanner.Mappers;

using TomLonghurst.PullRequestScanner.Enums;

public static class PullRequestStatusMapper
{
    public static string GetMessage(this PullRequestStatus pullRequestStatus)
    {
        return pullRequestStatus switch
        {
            PullRequestStatus.OutStandingComments => "Comments",
            PullRequestStatus.NeedsReviewing => "Needs reviewing",
            PullRequestStatus.MergeConflicts => "Merge conflicts",
            PullRequestStatus.Rejected => "Rejected",
            PullRequestStatus.ReadyToMerge => "Ready to merge",
            PullRequestStatus.FailingChecks => "Failing checks",
            PullRequestStatus.Completed => "Completed",
            PullRequestStatus.Abandoned => "Abandoned",
            PullRequestStatus.Draft => "Draft",
            PullRequestStatus.FailedToMerge => "Failed to merge",
            _ => throw new ArgumentOutOfRangeException(nameof(pullRequestStatus), pullRequestStatus, null),
        };
    }
}