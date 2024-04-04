// <copyright file="IPullRequestScanner.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.Services;

using TomLonghurst.PullRequestScanner.Contracts;
using TomLonghurst.PullRequestScanner.Models;

public interface IPullRequestScanner : IHasPlugins
{
    Task<IReadOnlyList<PullRequest>> GetPullRequests();

    Task ExecutePluginsAsync() => this.ExecutePluginsAsync(null as Func<IPullRequestPlugin, bool>);

    Task ExecutePluginsAsync(IReadOnlyList<PullRequest> pullRequests) => this.ExecutePluginsAsync(pullRequests, null);

    Task ExecutePluginsAsync(Func<IPullRequestPlugin, bool>? predicate);

    Task ExecutePluginsAsync(IReadOnlyList<PullRequest> pullRequests, Func<IPullRequestPlugin, bool>? predicate);
}