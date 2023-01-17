using TomLonghurst.EnumerableAsyncProcessor.Extensions;
using TomLonghurst.PullRequestScanner.Contracts;
using TomLonghurst.PullRequestScanner.Exceptions;
using TomLonghurst.PullRequestScanner.Models;

namespace TomLonghurst.PullRequestScanner.Services;

internal class PluginService : IPluginService
{
    private readonly IEnumerable<IPullRequestPlugin> _plugins;
    
    private readonly Func<IPullRequestPlugin, bool> _defaultPredicate = _ => true;

    public PluginService(IEnumerable<IPullRequestPlugin> plugins)
    {
        _plugins = plugins;
    }

    public async Task ExecuteAsync(IReadOnlyList<PullRequest> pullRequests, Func<IPullRequestPlugin, bool>? predicate = null)
    {
        if (!_plugins.Any())
        {
            throw new NoPullRequestPluginsRegisteredException();
        }

        await _plugins
            .Where(predicate ?? _defaultPredicate)
            .ToAsyncProcessorBuilder()
            .ForEachAsync(plugin => plugin.ExecuteAsync(pullRequests))
            .ProcessInParallel();
    }
}