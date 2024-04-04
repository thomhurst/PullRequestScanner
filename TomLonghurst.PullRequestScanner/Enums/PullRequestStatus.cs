namespace TomLonghurst.PullRequestScanner.Enums;

public enum PullRequestStatus
{
    FailingChecks,
    OutStandingComments,
    NeedsReviewing,
    MergeConflicts,
    Rejected,
    ReadyToMerge,
    Completed,
    Abandoned,
    Draft,
    FailedToMerge,
}