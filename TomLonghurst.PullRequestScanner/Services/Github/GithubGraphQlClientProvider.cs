using System.Reflection;
using Octokit.GraphQL;

namespace TomLonghurst.PullRequestScanner.Services.Github;

internal class GithubGraphQlClientProvider : IGithubGraphQlClientProvider
{
    public Connection GithubGraphQlClient { get; }

    public GithubGraphQlClientProvider(PullRequestScannerOptions pullRequestScannerOptions)
    {
        var version = Assembly.GetAssembly(typeof(GithubGraphQlClientProvider))?.GetName()?.Version?.ToString() ?? "1.0";
        GithubGraphQlClient = new Connection(new ProductHeaderValue("pr-scanner", version), pullRequestScannerOptions.Github.PersonalAccessToken);
    }
}