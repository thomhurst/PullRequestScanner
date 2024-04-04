// <copyright file="IPullRequestProvider.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.Contracts;

using TomLonghurst.PullRequestScanner.Models;

public interface IPullRequestProvider
{
    Task<IReadOnlyList<PullRequest>> GetPullRequests();
}