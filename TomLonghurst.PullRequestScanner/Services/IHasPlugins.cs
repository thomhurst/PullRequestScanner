using TomLonghurst.PullRequestScanner.Contracts;

namespace TomLonghurst.PullRequestScanner.Services;

public interface IHasPlugins
{
    IEnumerable<IPullRequestPlugin> Plugins { get; }
}