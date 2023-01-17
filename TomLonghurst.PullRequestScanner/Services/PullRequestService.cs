using System.Collections.Immutable;
using Microsoft.Extensions.Caching.Memory;
using TomLonghurst.PullRequestScanner.Contracts;
using TomLonghurst.PullRequestScanner.Exceptions;
using TomLonghurst.PullRequestScanner.Models;

namespace TomLonghurst.PullRequestScanner.Services;

internal class PullRequestService : IPullRequestService
{
    private const string PullRequestsCacheKey = "PullRequests";
    
    private readonly IEnumerable<IPullRequestProvider> _pullRequestProviders;
    private readonly IEnumerable<IPullRequestScannerInitializer> _initializers;
    private readonly IMemoryCache _memoryCache;
    private readonly SemaphoreSlim Lock = new(1, 1);

    public PullRequestService(IEnumerable<IPullRequestProvider> pullRequestProviders,
        IEnumerable<IPullRequestScannerInitializer> initializers,
        IMemoryCache memoryCache)
    {
        _pullRequestProviders = pullRequestProviders;
        _initializers = initializers;
        _memoryCache = memoryCache;
    }

    public async Task<IReadOnlyList<PullRequest>> GetPullRequests()
    {
        if (!_pullRequestProviders.Any())
        {
            throw new NoPullRequestProvidersRegisteredException();
        }
        
        await Lock.WaitAsync();

        try
        {
            await Task.WhenAll(_initializers.Select(x => x.Initialize()));
            
            if (_memoryCache.TryGetValue(PullRequestsCacheKey, out ImmutableList<PullRequest> prs))
            {
                return prs;
            }

            var pullRequests = await Task.WhenAll(_pullRequestProviders.Select(x => x.GetPullRequests()));
            var pullRequestsImmutableList = pullRequests.SelectMany(x => x).ToImmutableList();

            _memoryCache.Set(PullRequestsCacheKey, pullRequestsImmutableList, TimeSpan.FromMinutes(5));
            
            return pullRequestsImmutableList;
        }
        finally
        {
            Lock.Release();
        }
    }
}