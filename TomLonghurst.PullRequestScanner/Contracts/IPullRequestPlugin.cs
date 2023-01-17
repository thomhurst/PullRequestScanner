using TomLonghurst.PullRequestScanner.Models;

namespace TomLonghurst.PullRequestScanner.Contracts;

public interface IPullRequestPlugin
{
    Task ExecuteAsync(IReadOnlyList<PullRequest> pullRequests);
}