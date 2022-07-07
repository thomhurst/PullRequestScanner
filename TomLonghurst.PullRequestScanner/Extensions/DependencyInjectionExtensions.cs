using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using TomLonghurst.PullRequestScanner.Http;
using TomLonghurst.PullRequestScanner.Mappers;
using TomLonghurst.PullRequestScanner.Options;
using TomLonghurst.PullRequestScanner.Services;
using TomLonghurst.PullRequestScanner.Services.DevOps;
using TomLonghurst.PullRequestScanner.Services.Github;

namespace TomLonghurst.PullRequestScanner.Extensions;

public static class DependencyInjectionExtensions
{
    internal static IServiceCollection AddStartupInitializer<TImplementation>(this IServiceCollection services)
        where TImplementation : class, IInitialize
    {
        return services.AddTransient<IInitialize>(rootServiceProvider =>
        {
            var serviceProvider = rootServiceProvider.CreateAsyncScope().ServiceProvider;
            var startupInitializer = serviceProvider.GetService<TImplementation>();

            if (startupInitializer is null)
            {
                var firstInterface = typeof(TImplementation).GetInterfaces().FirstOrDefault(i => i != typeof(IInitialize));
                if (firstInterface != null)
                {
                    startupInitializer = serviceProvider.GetService(firstInterface) as TImplementation;
                }
            }

            if (startupInitializer is null)
            {
                startupInitializer = ActivatorUtilities.CreateInstance<TImplementation>(serviceProvider);
            }

            return startupInitializer;
        });
    }

    public static IServiceCollection AddPullRequestScanner(this IServiceCollection services, PullRequestScannerOptions pullRequestScannerOptions)
    {
        services.AddSingleton(pullRequestScannerOptions);
        
        services
            .AddHttpClient<DevOpsHttpClient>(client =>
            {
                var azureDevOpsOptions = pullRequestScannerOptions.AzureDevOps;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(Encoding.ASCII.GetBytes(azureDevOpsOptions.PersonalAccessToken)));
                client.BaseAddress = new Uri($"https://dev.azure.com/{azureDevOpsOptions.OrganizationSlug}/{azureDevOpsOptions.ProjectSlug}/_apis/git/");
            });

        services
            .AddHttpClient<GithubHttpClient>(client =>
            {
                var githubOptions = pullRequestScannerOptions.Github;
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("pull-request-scanner", Assembly.GetAssembly(typeof(PullRequestService)).GetName().Version.ToString()));

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(Encoding.ASCII.GetBytes(githubOptions.PersonalAccessToken)));
                client.BaseAddress = new Uri("https://api.github.com/");
            });

        services
            .AddHttpClient<MicrosoftTeamsWebhookClient>();
        
        services.AddStartupInitializer<TeamMembersService>();
        
        services.AddTransient<IDevOpsGitRepositoryService, DevOpsGitRepositoryService>()
            .AddTransient<IDevOpsPullRequestService, DevOpsPullRequestService>()
            .AddTransient<IDevOpsMapper, DevOpsMapper>();
        
        services
            .AddSingleton<IGithubGraphQlClientProvider, GithubGraphQlClientProvider>()
            .AddSingleton<IGithubUserService, GithubUserService>()
            .AddSingleton<IDevOpsUserService, DevOpsUserService>()
            .AddSingleton<ITeamMembersService, TeamMembersService>()
            .AddTransient<IGithubRepositoryService, GithubRepositoryService>()
            .AddTransient<IGithubPullRequestService, GithubPullRequestService>()
            .AddTransient<IGithubMapper, GithubMapper>();

        services
            .AddTransient<IPullRequestService, PullRequestService>()
            .AddTransient<IPullRequestScannerNotifier, PullRequestScannerNotifier>()
            .AddTransient<PullRequestScannerNotifier>();

        return services;
    }
}