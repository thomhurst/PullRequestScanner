// <copyright file="PullRequestStatus.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

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