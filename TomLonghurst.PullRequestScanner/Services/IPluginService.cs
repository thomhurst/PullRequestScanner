using TomLonghurst.PullRequestScanner.Contracts;
using TomLonghurst.PullRequestScanner.Models;

namespace TomLonghurst.PullRequestScanner.Services;

internal interface IPluginService : IHasPlugins
{
    Task ExecuteAsync(IReadOnlyList<PullRequest> pullRequests) => ExecuteAsync(pullRequests, null);
    Task ExecuteAsync(IReadOnlyList<PullRequest> pullRequests, Func<IPullRequestPlugin, bool>? predicate);
}