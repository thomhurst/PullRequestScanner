// <copyright file="PullRequestService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.Services;

using System.Collections.Immutable;
using Initialization.Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Caching.Memory;
using TomLonghurst.PullRequestScanner.Contracts;
using TomLonghurst.PullRequestScanner.Exceptions;
using TomLonghurst.PullRequestScanner.Models;

internal class PullRequestService : IPullRequestService
{
    private const string PullRequestsCacheKey = "PullRequests";

    private readonly IEnumerable<IPullRequestProvider> pullRequestProviders;
    private readonly IServiceProvider serviceProvider;
    private readonly IMemoryCache memoryCache;
    private readonly SemaphoreSlim @lock = new(1, 1);

    public PullRequestService(
        IEnumerable<IPullRequestProvider> pullRequestProviders,
        IServiceProvider serviceProvider,
        IMemoryCache memoryCache)
    {
        this.pullRequestProviders = pullRequestProviders;
        this.serviceProvider = serviceProvider;
        this.memoryCache = memoryCache;
    }

    public async Task<IReadOnlyList<PullRequest>> GetPullRequests()
    {
        if (!this.pullRequestProviders.Any())
        {
            throw new NoPullRequestProvidersRegisteredException();
        }

        await this.@lock.WaitAsync();

        try
        {
            await this.Initialize();

            if (this.memoryCache.TryGetValue(PullRequestsCacheKey, out ImmutableList<PullRequest> prs))
            {
                return prs;
            }

            var pullRequests = await Task.WhenAll(this.pullRequestProviders.Select(x => x.GetPullRequests()));

            var pullRequestsImmutableList = pullRequests
                .SelectMany(x => x)
                .Where(x => x.Labels?.Contains(Constants.PullRequestScannerIgnoreTag, StringComparer.CurrentCultureIgnoreCase) != true)
                .ToImmutableList();

            this.memoryCache.Set(PullRequestsCacheKey, pullRequestsImmutableList, TimeSpan.FromMinutes(5));

            return pullRequestsImmutableList;
        }
        finally
        {
            this.@lock.Release();
        }
    }

    private async Task Initialize()
    {
        await this.serviceProvider.InitializeAsync();
    }
}