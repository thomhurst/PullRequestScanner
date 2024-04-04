namespace TomLonghurst.PullRequestScanner.Services;

using TomLonghurst.PullRequestScanner.Contracts;
using TomLonghurst.PullRequestScanner.Models;

internal interface IPluginService : IHasPlugins
{
    Task ExecuteAsync(IReadOnlyList<PullRequest> pullRequests) => this.ExecuteAsync(pullRequests, null);

    Task ExecuteAsync(IReadOnlyList<PullRequest> pullRequests, Func<IPullRequestPlugin, bool>? predicate);
}