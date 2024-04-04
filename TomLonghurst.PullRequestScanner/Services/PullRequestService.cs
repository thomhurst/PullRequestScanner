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
        if (!pullRequestProviders.Any())
        {
            throw new NoPullRequestProvidersRegisteredException();
        }

        await @lock.WaitAsync();

        try
        {
            await Initialize();

            if (memoryCache.TryGetValue(PullRequestsCacheKey, out ImmutableList<PullRequest> prs))
            {
                return prs;
            }

            var pullRequests = await Task.WhenAll(pullRequestProviders.Select(x => x.GetPullRequests()));

            var pullRequestsImmutableList = pullRequests
                .SelectMany(x => x)
                .Where(x => x.Labels?.Contains(Constants.PullRequestScannerIgnoreTag, StringComparer.CurrentCultureIgnoreCase) != true)
                .ToImmutableList();

            memoryCache.Set(PullRequestsCacheKey, pullRequestsImmutableList, TimeSpan.FromMinutes(5));

            return pullRequestsImmutableList;
        }
        finally
        {
            @lock.Release();
        }
    }

    private async Task Initialize()
    {
        await serviceProvider.InitializeAsync();
    }
}