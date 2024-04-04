// <copyright file="IPullRequestPlugin.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.Contracts;

using TomLonghurst.PullRequestScanner.Models;

public interface IPullRequestPlugin
{
    Task ExecuteAsync(IReadOnlyList<PullRequest> pullRequests);
}