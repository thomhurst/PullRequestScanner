namespace TomLonghurst.PullRequestScanner.Services;

using TomLonghurst.PullRequestScanner.Contracts;

public interface IHasPlugins
{
    IEnumerable<IPullRequestPlugin> Plugins { get; }

    TPlugin GetPlugin<TPlugin>()
        where TPlugin : IPullRequestPlugin;
}