namespace TomLonghurst.PullRequestScanner.GitHub.Services;

using System.Reflection;
using Octokit.GraphQL;
using Options;

internal class GithubGraphQlClientProvider : IGithubGraphQlClientProvider
{
    public Connection GithubGraphQlClient { get; }

    public GithubGraphQlClientProvider(GithubOptions githubOptions)
    {
        var version = Assembly.GetAssembly(typeof(GithubGraphQlClientProvider))?.GetName()?.Version?.ToString() ?? "1.0";

        var accessToken = githubOptions.PersonalAccessToken;

        if (accessToken.Contains(':'))
        {
            accessToken = accessToken.Split(':').Last();
        }

        GithubGraphQlClient = new Connection(new ProductHeaderValue("pr-scanner", version), accessToken);
    }
}