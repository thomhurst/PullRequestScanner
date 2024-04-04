// <copyright file="DevOpsPullRequestContext.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Models;

using Microsoft.TeamFoundation.SourceControl.WebApi;

internal class AzureDevOpsPullRequestContext
{
    public GitPullRequest AzureDevOpsPullRequest { get; set; }

    public List<GitPullRequestCommentThread> PullRequestThreads { get; set; }

    public List<GitPullRequestStatus> Iterations { get; set; }
}