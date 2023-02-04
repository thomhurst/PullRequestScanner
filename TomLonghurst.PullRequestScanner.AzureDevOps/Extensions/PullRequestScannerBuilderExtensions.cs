using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using TomLonghurst.PullRequestScanner.AzureDevOps.Http;
using TomLonghurst.PullRequestScanner.AzureDevOps.Mappers;
using TomLonghurst.PullRequestScanner.AzureDevOps.Options;
using TomLonghurst.PullRequestScanner.AzureDevOps.Services;
using TomLonghurst.PullRequestScanner.Contracts;
using TomLonghurst.PullRequestScanner.Extensions;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Extensions;

public static class PullRequestScannerBuilderExtensions
{
    public static PullRequestScannerBuilder AddAzureDevOps(this PullRequestScannerBuilder pullRequestScannerBuilder,
        AzureDevOpsOptions azureDevOpsOptions)
    {
        pullRequestScannerBuilder.Services.AddSingleton(azureDevOpsOptions);
        return AddAzureDevOps(pullRequestScannerBuilder);
    }

    public static PullRequestScannerBuilder AddAzureDevOps(this PullRequestScannerBuilder pullRequestScannerBuilder,
        Func<IServiceProvider, AzureDevOpsOptions> azureDevOpsOptionsFactory)
    {
        pullRequestScannerBuilder.Services.AddSingleton(azureDevOpsOptionsFactory);
        return AddAzureDevOps(pullRequestScannerBuilder);
    }

    private static PullRequestScannerBuilder AddAzureDevOps(this PullRequestScannerBuilder pullRequestScannerBuilder)
    {
        pullRequestScannerBuilder.Services
            .AddHttpClient<AzureDevOpsHttpClient>((provider, client) =>
            {
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                var azureDevOpsOptions = provider.GetRequiredService<AzureDevOpsOptions>();


                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(Encoding.ASCII.GetBytes(FormatAccessToken(azureDevOpsOptions.PersonalAccessToken))));

                client.BaseAddress =
                    new Uri(
                        $"https://dev.azure.com/{azureDevOpsOptions.OrganizationSlug}/");
            });

        pullRequestScannerBuilder.Services.AddTransient<IAzureDevOpsGitRepositoryService, AzureDevOpsGitRepositoryService>()
            .AddTransient<IAzureDevOpsPullRequestService, AzureDevOpsPullRequestService>()
            .AddTransient<IAzureDevOpsMapper, AzureDevOpsMapper>()
            .AddSingleton<ITeamMembersProvider, AzureDevOpsTeamMembersProvider>();

        return pullRequestScannerBuilder.AddPullRequestProvider(ActivatorUtilities.GetServiceOrCreateInstance<AzureDevOpsPullRequestProvider>);
    }

    private static string FormatAccessToken(string personalAccessToken)
    {
        if (personalAccessToken == null)
        {
            throw new ArgumentNullException(nameof(personalAccessToken));
        }

        if (!personalAccessToken.Contains(':'))
        {
            return $":{personalAccessToken}";
        }

        return personalAccessToken;
    }
}