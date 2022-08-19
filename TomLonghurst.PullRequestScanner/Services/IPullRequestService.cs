using TomLonghurst.PullRequestScanner.Models.Self;

namespace TomLonghurst.PullRequestScanner.Services;

public interface IPullRequestService
{
    Task<IReadOnlyList<PullRequest>> GetPullRequests();
}