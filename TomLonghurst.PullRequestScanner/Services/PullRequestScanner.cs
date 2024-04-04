// <copyright file="PullRequestScanner.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.Services;

using TomLonghurst.PullRequestScanner.Contracts;
using TomLonghurst.PullRequestScanner.Models;

internal class PullRequestScanner : IPullRequestScanner
{
    private readonly IPullRequestService pullRequestService;
    private readonly IPluginService pluginService;

    public IEnumerable<IPullRequestPlugin> Plugins => this.pluginService.Plugins;

    public TPlugin GetPlugin<TPlugin>()
        where TPlugin : IPullRequestPlugin
        => this.pluginService.GetPlugin<TPlugin>();

    public PullRequestScanner(IPullRequestService pullRequestService, IPluginService pluginService)
    {
        this.pullRequestService = pullRequestService;
        this.pluginService = pluginService;
    }

    public Task<IReadOnlyList<PullRequest>> GetPullRequests()
    {
        return this.pullRequestService.GetPullRequests();
    }

    public async Task ExecutePluginsAsync(Func<IPullRequestPlugin, bool>? predicate)
    {
        var pullRequests = await this.GetPullRequests();

        await this.ExecutePluginsAsync(pullRequests, predicate);
    }

    public Task ExecutePluginsAsync(IReadOnlyList<PullRequest> pullRequests, Func<IPullRequestPlugin, bool>? predicate)
    {
        if (pullRequests == null)
        {
            throw new ArgumentNullException(nameof(pullRequests));
        }

        return this.pluginService.ExecuteAsync(pullRequests, predicate);
    }
}