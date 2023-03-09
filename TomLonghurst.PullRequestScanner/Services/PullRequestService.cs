﻿using System.Collections.Immutable;
using Microsoft.Extensions.Caching.Memory;
using TomLonghurst.Microsoft.Extensions.DependencyInjection.ServiceInitialization;
using TomLonghurst.PullRequestScanner.Contracts;
using TomLonghurst.PullRequestScanner.Exceptions;
using TomLonghurst.PullRequestScanner.Models;

namespace TomLonghurst.PullRequestScanner.Services;

internal class PullRequestService : IPullRequestService
{
    private const string PullRequestsCacheKey = "PullRequests";
    
    private readonly IEnumerable<IPullRequestProvider> _pullRequestProviders;
    private readonly IEnumerable<IInitializer> _initializers;
    private readonly IEnumerable<ITeamMembersService> _teamMembersServices;
    private readonly IMemoryCache _memoryCache;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public PullRequestService(IEnumerable<IPullRequestProvider> pullRequestProviders,
        IEnumerable<IInitializer> initializers,
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
        foreach (var orderedInitializerGroup in _initializers.GroupBy(x => x.Order).OrderBy(x => x.Key))
        {
            await Task.WhenAll(orderedInitializerGroup.Select(x => x.InitializeAsync()));   
        }
    }
}