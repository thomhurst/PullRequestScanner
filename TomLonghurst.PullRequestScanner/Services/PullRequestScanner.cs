using TomLonghurst.PullRequestScanner.Contracts;
using TomLonghurst.PullRequestScanner.Models;

namespace TomLonghurst.PullRequestScanner.Services;

internal class PullRequestScanner : IPullRequestScanner
{
    private readonly IPullRequestService _pullRequestService;
    private readonly IPluginService _pluginService;

    public IEnumerable<IPullRequestPlugin> Plugins => _pluginService.Plugins;
    public TPlugin GetPlugin<TPlugin>() where TPlugin : IPullRequestPlugin => _pluginService.GetPlugin<TPlugin>();

    public PullRequestScanner(IPullRequestService pullRequestService, IPluginService pluginService)
    {
        _pullRequestService = pullRequestService;
        _pluginService = pluginService;
    }

    public Task<IReadOnlyList<PullRequest>> GetPullRequests()
    {
        return _pullRequestService.GetPullRequests();
    }

    public async Task ExecutePluginsAsync(Func<IPullRequestPlugin, bool>? predicate)
    {
        var pullRequests = await GetPullRequests();

        await ExecutePluginsAsync(pullRequests, predicate);
    }

    public Task ExecutePluginsAsync(IReadOnlyList<PullRequest> pullRequests, Func<IPullRequestPlugin, bool>? predicate)
    {
        if (pullRequests == null)
        {
            throw new ArgumentNullException(nameof(pullRequests));
        }

        return _pluginService.ExecuteAsync(pullRequests, predicate);
    }
}