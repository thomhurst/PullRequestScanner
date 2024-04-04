namespace TomLonghurst.PullRequestScanner.AzureDevOps.Extensions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Mappers;
using Options;
using Services;
using Contracts;
using TomLonghurst.PullRequestScanner.Extensions;

public static class PullRequestScannerBuilderExtensions
{
    public static PullRequestScannerBuilder AddAzureDevOps(
        this PullRequestScannerBuilder pullRequestScannerBuilder,
        AzureDevOpsOptions azureDevOpsOptions)
    {
        pullRequestScannerBuilder.Services.AddSingleton(azureDevOpsOptions);
        return AddAzureDevOps(pullRequestScannerBuilder);
    }

    public static PullRequestScannerBuilder AddAzureDevOps(
        this PullRequestScannerBuilder pullRequestScannerBuilder,
        Func<IServiceProvider, AzureDevOpsOptions> azureDevOpsOptionsFactory)
    {
        pullRequestScannerBuilder.Services.AddSingleton(azureDevOpsOptionsFactory);
        return AddAzureDevOps(pullRequestScannerBuilder);
    }

    private static PullRequestScannerBuilder AddAzureDevOps(this PullRequestScannerBuilder pullRequestScannerBuilder)
    {
        pullRequestScannerBuilder.Services.AddSingleton(sp =>
        {
            var azureDevOpsOptions = sp.GetRequiredService<AzureDevOpsOptions>();

            var uri = new UriBuilder("https://dev.azure.com/")
            {
                Path = $"/{azureDevOpsOptions.Organization}",
            }.Uri;

            return new VssConnection(uri, new VssBasicCredential(string.Empty, azureDevOpsOptions?.PersonalAccessToken));
        });

        pullRequestScannerBuilder.Services.AddTransient<IAzureDevOpsGitRepositoryService, AzureDevOpsGitRepositoryService>()
            .AddTransient<IAzureDevOpsPullRequestService, AzureDevOpsPullRequestService>()
            .AddTransient<IAzureDevOpsMapper, AzureDevOpsMapper>()
            .AddSingleton<ITeamMembersProvider, AzureDevOpsTeamMembersProvider>()
            .AddSingleton<AzureDevOpsInitializer>();

        return pullRequestScannerBuilder.AddPullRequestProvider(ActivatorUtilities.GetServiceOrCreateInstance<AzureDevOpsPullRequestProvider>);
    }
}