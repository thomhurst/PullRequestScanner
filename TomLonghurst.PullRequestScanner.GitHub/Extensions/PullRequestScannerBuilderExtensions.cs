using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using TomLonghurst.PullRequestScanner.Contracts;
using TomLonghurst.PullRequestScanner.Extensions;
using TomLonghurst.PullRequestScanner.GitHub.Http;
using TomLonghurst.PullRequestScanner.GitHub.Mappers;
using TomLonghurst.PullRequestScanner.GitHub.Options;
using TomLonghurst.PullRequestScanner.GitHub.Services;
using TomLonghurst.PullRequestScanner.Services;

namespace TomLonghurst.PullRequestScanner.GitHub.Extensions;

public static class PullRequestScannerBuilderExtensions
{
    public static PullRequestScannerBuilder AddGithub(this PullRequestScannerBuilder pullRequestScannerBuilder,
        GithubOptions githubOptions)
    {
        pullRequestScannerBuilder.Services.AddSingleton(githubOptions);
        return AddGithub(pullRequestScannerBuilder);
    }

    public static PullRequestScannerBuilder AddGithub(this PullRequestScannerBuilder pullRequestScannerBuilder,
        Func<IServiceProvider, GithubOptions> githubOptionsFactory)
    {
        pullRequestScannerBuilder.Services.AddSingleton(githubOptionsFactory);
        return AddGithub(pullRequestScannerBuilder);
    }

    private static PullRequestScannerBuilder AddGithub(this PullRequestScannerBuilder pullRequestScannerBuilder)
    {
        pullRequestScannerBuilder.Services
            .AddHttpClient<GithubHttpClient>((provider, client) =>
            {
                var githubOptions = provider.GetRequiredService<GithubOptions>();
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("TomLonghurst.PullRequestScanner", Assembly.GetAssembly(typeof(IPullRequestScanner)).GetName().Version.ToString()));

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(Encoding.ASCII.GetBytes(githubOptions.PersonalAccessToken)));
                client.BaseAddress = new Uri("https://api.github.com/");
            });

        pullRequestScannerBuilder.Services
            .AddSingleton<IGithubGraphQlClientProvider, GithubGraphQlClientProvider>()
            .AddSingleton<IGithubQueryRunner, GithubQueryRunner>()
            .AddSingleton<ITeamMembersProvider, GithubTeamMembersProvider>()
            .AddTransient<IGithubRepositoryService, GithubRepositoryService>()
            .AddTransient<IGithubPullRequestService, GithubPullRequestService>()
            .AddTransient<IGithubMapper, GithubMapper>();

        return pullRequestScannerBuilder.AddPullRequestProvider(ActivatorUtilities.GetServiceOrCreateInstance<GitHubPullRequestProvider>);
    }
}