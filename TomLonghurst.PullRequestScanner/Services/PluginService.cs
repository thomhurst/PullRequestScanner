﻿namespace TomLonghurst.PullRequestScanner.Services;

using EnumerableAsyncProcessor.Extensions;
using Contracts;
using Exceptions;
using Models;

internal class PluginService : IPluginService
{
    public IEnumerable<IPullRequestPlugin> Plugins { get; }

    private readonly Func<IPullRequestPlugin, bool> _defaultPredicate = _ => true;

    public PluginService(IEnumerable<IPullRequestPlugin> plugins)
    {
        Plugins = plugins;
    }

    public async Task ExecuteAsync(IReadOnlyList<PullRequest> pullRequests, Func<IPullRequestPlugin, bool>? predicate = null)
    {
        if (!Plugins.Any())
        {
            throw new NoPullRequestPluginsRegisteredException();
        }

        await Plugins
            .Where(predicate ?? _defaultPredicate)
            .ToAsyncProcessorBuilder()
            .ForEachAsync(plugin => plugin.ExecuteAsync(pullRequests))
            .ProcessInParallel();
    }

    public TPlugin GetPlugin<TPlugin>()
        where TPlugin : IPullRequestPlugin
    {
        return Plugins.OfType<TPlugin>().Single();
    }
}