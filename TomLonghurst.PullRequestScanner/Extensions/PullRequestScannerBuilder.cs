namespace TomLonghurst.PullRequestScanner.Extensions;

using Initialization.Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Contracts;
using Services;

public class PullRequestScannerBuilder
{
    public IServiceCollection Services { get; }

    internal PullRequestScannerBuilder(IServiceCollection services)
    {
        Services = services;

        Services
            .AddTransient<IPullRequestService, PullRequestService>()
            .AddTransient<IPluginService, PluginService>()
            .AddSingleton<ITeamMembersService, TeamMembersService>()
            .AddTransient<IPullRequestScanner, PullRequestScanner>();

        Services.AddLogging();

        Services.AddInitializers();

        Services.AddMemoryCache();
    }

    public PullRequestScannerBuilder AddPullRequestProvider(Func<IServiceProvider, IPullRequestProvider> pullRequestProviderFactory)
    {
        Services.AddTransient(pullRequestProviderFactory);
        return this;
    }

    public PullRequestScannerBuilder AddPullRequestProvider<TPullRequestProvider>()
        where TPullRequestProvider : class, IPullRequestProvider
    {
        Services.AddTransient<IPullRequestProvider, TPullRequestProvider>();
        return this;
    }

    public PullRequestScannerBuilder AddPlugin(Func<IServiceProvider, IPullRequestPlugin> pullRequestPluginFactory)
    {
        Services.AddTransient(pullRequestPluginFactory);
        return this;
    }

    public PullRequestScannerBuilder AddPlugin<TPullRequestPlugin>()
        where TPullRequestPlugin : class, IPullRequestPlugin
    {
        Services.AddTransient<IPullRequestPlugin, TPullRequestPlugin>();
        return this;
    }
}