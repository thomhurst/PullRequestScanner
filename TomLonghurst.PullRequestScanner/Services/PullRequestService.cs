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

    private readonly IEnumerable<IPullRequestProvider> _pullRequestProviders;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMemoryCache _memoryCache;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public PullRequestService(
        IEnumerable<IPullRequestProvider> pullRequestProviders,
        IServiceProvider serviceProvider,
        IMemoryCache memoryCache)
    {
        this._pullRequestProviders = pullRequestProviders;
        this._serviceProvider = serviceProvider;
        this._memoryCache = memoryCache;
    }

    public async Task<IReadOnlyList<PullRequest>> GetPullRequests()
    {
        if (!_pullRequestProviders.Any())
        {
            throw new NoPullRequestProvidersRegisteredException();
        }

        await _lock.WaitAsync();

        try
        {
            await Initialize();

            if (_memoryCache.TryGetValue(PullRequestsCacheKey, out ImmutableList<PullRequest> prs))
            {
                return prs;
            }

            var pullRequests = await Task.WhenAll(_pullRequestProviders.Select(x => x.GetPullRequests()));

            var pullRequestsImmutableList = pullRequests
                .SelectMany(x => x)
                .Where(x => x.Labels?.Contains(Constants.PullRequestScannerIgnoreTag, StringComparer.CurrentCultureIgnoreCase) != true)
                .ToImmutableList();

            _memoryCache.Set(PullRequestsCacheKey, pullRequestsImmutableList, TimeSpan.FromMinutes(5));

            return pullRequestsImmutableList;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task Initialize()
    {
        await _serviceProvider.InitializeAsync();
    }
}