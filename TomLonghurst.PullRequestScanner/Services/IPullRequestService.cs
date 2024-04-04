// <copyright file="IPullRequestService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.Services;

using TomLonghurst.PullRequestScanner.Models;

internal interface IPullRequestService
{
    Task<IReadOnlyList<PullRequest>> GetPullRequests();
}