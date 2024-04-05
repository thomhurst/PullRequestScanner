namespace TomLonghurst.PullRequestScanner.Exceptions;

public class NoPullRequestPluginsRegisteredException : PullRequestScannerException
{
    public NoPullRequestPluginsRegisteredException()
        : base("No Pull Request Plugins have been registered")
    {
    }
}