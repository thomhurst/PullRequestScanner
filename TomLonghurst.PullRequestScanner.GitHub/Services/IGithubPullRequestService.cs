// <copyright file="IGithubPullRequestService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.GitHub.Services;

using TomLonghurst.PullRequestScanner.GitHub.Models;

internal interface IGithubPullRequestService
{
    Task<IEnumerable<GithubPullRequest>> GetPullRequests(GithubRepository repository);
}