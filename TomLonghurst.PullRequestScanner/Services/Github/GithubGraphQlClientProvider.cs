using System.Reflection;
using Octokit.GraphQL;

namespace TomLonghurst.PullRequestScanner.Services.Github;

internal class GithubGraphQlClientProvider : IGithubGraphQlClientProvider
{
    public Connection GithubGraphQlClient { get; }

    public GithubGraphQlClientProvider(PullRequestScannerOptions pullRequestScannerOptions)
    {
        var version = Assembly.GetAssembly(typeof(GithubGraphQlClientProvider))?.GetName()?.Version?.ToString() ?? "1.0";
        
        var accessToken = pullRequestScannerOptions.Github.PersonalAccessToken;

        if (accessToken.Contains(':'))
        {
            accessToken = accessToken.Split(':').Last();
        }
        
        GithubGraphQlClient = new Connection(new ProductHeaderValue("pr-scanner", version), accessToken);
    }
}