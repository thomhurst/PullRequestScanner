using Microsoft.Extensions.DependencyInjection;
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
        
        Services.AddStartupInitializer<TeamMembersService>();
        
        Services.AddMemoryCache();
    }

    public PullRequestScannerBuilder AddPullRequestProvider(Func<IServiceProvider, IPullRequestProvider> pullRequestProviderFactory)
    {
        Services.AddTransient(pullRequestProviderFactory);
        return this;
    }
    
    public PullRequestScannerBuilder AddPlugin(Func<IServiceProvider, IPullRequestPlugin> pullRequestPluginFactory)
    {
        Services.AddTransient(pullRequestPluginFactory);
        return this;
    }
}