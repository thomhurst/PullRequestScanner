namespace TomLonghurst.PullRequestScanner.Services;

using Contracts;
using Models;

internal interface IPluginService : IHasPlugins
{
    Task ExecuteAsync(IReadOnlyList<PullRequest> pullRequests) => ExecuteAsync(pullRequests, null);

    Task ExecuteAsync(IReadOnlyList<PullRequest> pullRequests, Func<IPullRequestPlugin, bool>? predicate);
}