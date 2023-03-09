using Microsoft.Extensions.DependencyInjection;
using TomLonghurst.Microsoft.Extensions.DependencyInjection.ServiceInitialization.Extensions;
using TomLonghurst.PullRequestScanner.Contracts;
using TomLonghurst.PullRequestScanner.Services;

namespace TomLonghurst.PullRequestScanner.Extensions;

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
            .AddTransient<IPullRequestScanner, Services.PullRequestScanner>();

        Services.AddLogging();

        services.AddInitializers();

        Services.AddMemoryCache();
    }

    public PullRequestScannerBuilder AddPullRequestProvider(Func<IServiceProvider, IPullRequestProvider> pullRequestProviderFactory)
    {
        Services.AddTransient(pullRequestProviderFactory);
        return this;
    }
    
    public PullRequestScannerBuilder AddPullRequestProvider<TPullRequestProvider>() where TPullRequestProvider : class, IPullRequestProvider
    {
        Services.AddTransient<IPullRequestProvider, TPullRequestProvider>();
        return this;
    }
    
    public PullRequestScannerBuilder AddPlugin(Func<IServiceProvider, IPullRequestPlugin> pullRequestPluginFactory)
    {
        Services.AddTransient(pullRequestPluginFactory);
        return this;
    }
    
    public PullRequestScannerBuilder AddPlugin<TPullRequestPlugin>() where TPullRequestPlugin : class, IPullRequestPlugin
    {
        Services.AddTransient<IPullRequestPlugin, TPullRequestPlugin>();
        return this;
    }
}