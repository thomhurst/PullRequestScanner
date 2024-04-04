namespace TomLonghurst.PullRequestScanner.Services;

using TomLonghurst.PullRequestScanner.Contracts;
using TomLonghurst.PullRequestScanner.Models;

internal class PullRequestScanner : IPullRequestScanner
{
    private readonly IPullRequestService pullRequestService;
    private readonly IPluginService pluginService;

    public IEnumerable<IPullRequestPlugin> Plugins => pluginService.Plugins;

    public TPlugin GetPlugin<TPlugin>()
        where TPlugin : IPullRequestPlugin
        => pluginService.GetPlugin<TPlugin>();

    public PullRequestScanner(IPullRequestService pullRequestService, IPluginService pluginService)
    {
        this.pullRequestService = pullRequestService;
        this.pluginService = pluginService;
    }

    public Task<IReadOnlyList<PullRequest>> GetPullRequests()
    {
        return pullRequestService.GetPullRequests();
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

        return pluginService.ExecuteAsync(pullRequests, predicate);
    }
}