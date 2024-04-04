// <copyright file="PullRequestScannerBuilder.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.Extensions;

using Initialization.Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using TomLonghurst.PullRequestScanner.Contracts;
using TomLonghurst.PullRequestScanner.Services;

public class PullRequestScannerBuilder
{
    public IServiceCollection Services { get; }

    internal PullRequestScannerBuilder(IServiceCollection services)
    {
        this.Services = services;

        this.Services
            .AddTransient<IPullRequestService, PullRequestService>()
            .AddTransient<IPluginService, PluginService>()
            .AddSingleton<ITeamMembersService, TeamMembersService>()
            .AddTransient<IPullRequestScanner, Services.PullRequestScanner>();

        this.Services.AddLogging();

        this.Services.AddInitializers();

        this.Services.AddMemoryCache();
    }

    public PullRequestScannerBuilder AddPullRequestProvider(Func<IServiceProvider, IPullRequestProvider> pullRequestProviderFactory)
    {
        this.Services.AddTransient(pullRequestProviderFactory);
        return this;
    }

    public PullRequestScannerBuilder AddPullRequestProvider<TPullRequestProvider>()
        where TPullRequestProvider : class, IPullRequestProvider
    {
        this.Services.AddTransient<IPullRequestProvider, TPullRequestProvider>();
        return this;
    }

    public PullRequestScannerBuilder AddPlugin(Func<IServiceProvider, IPullRequestPlugin> pullRequestPluginFactory)
    {
        this.Services.AddTransient(pullRequestPluginFactory);
        return this;
    }

    public PullRequestScannerBuilder AddPlugin<TPullRequestPlugin>()
        where TPullRequestPlugin : class, IPullRequestPlugin
    {
        this.Services.AddTransient<IPullRequestPlugin, TPullRequestPlugin>();
        return this;
    }
}