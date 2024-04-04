namespace TomLonghurst.PullRequestScanner.Services;

using Contracts;
using Models;

public interface IPullRequestScanner : IHasPlugins
{
    Task<IReadOnlyList<PullRequest>> GetPullRequests();

    Task ExecutePluginsAsync() => ExecutePluginsAsync(null as Func<IPullRequestPlugin, bool>);

    Task ExecutePluginsAsync(IReadOnlyList<PullRequest> pullRequests) => ExecutePluginsAsync(pullRequests, null);

    Task ExecutePluginsAsync(Func<IPullRequestPlugin, bool>? predicate);

    Task ExecutePluginsAsync(IReadOnlyList<PullRequest> pullRequests, Func<IPullRequestPlugin, bool>? predicate);
}