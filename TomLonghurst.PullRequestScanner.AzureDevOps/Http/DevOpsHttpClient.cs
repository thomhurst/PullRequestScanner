using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using TomLonghurst.PullRequestScanner.Extensions;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Http;

internal class AzureDevOpsHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AzureDevOpsHttpClient> _logger;

    public AzureDevOpsHttpClient(HttpClient httpClient, ILogger<AzureDevOpsHttpClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    
    public async Task<T?> Get<T>(string path, CancellationToken cancellationToken = default)
    {
        var httpResponseMessage = await GetHttpResponseMessage(path, cancellationToken);

        return await httpResponseMessage
            .Content
            .ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<T>> GetAll<T>(string path, CancellationToken cancellationToken = default) where T : IHasCount
    {
        var results = new List<T>();

        var iteration = 0;
        do
        {
            var pathWithBatchingQueryParameters = path
                .AddQueryParam("$top", "100")
                .AddQueryParam("$skip", (100 * iteration).ToString());

            var response = await Get<T>(pathWithBatchingQueryParameters, cancellationToken);

            results.Add(response);
            
            iteration++;
        } while (results.Last().Count == 100 && (100*iteration) == results.Sum(r => r.Count));

        return results;
    }

    private async Task<HttpResponseMessage> GetHttpResponseMessage(string path, CancellationToken cancellationToken = default)
    {
        var response = await
            HttpPolicyExtensions.HandleTransientHttpError()
                .WaitAndRetryAsync(5, i => TimeSpan.FromSeconds(i * 2))
                .ExecuteAsync(() => _httpClient.GetAsync(path, cancellationToken));

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Error calling {Path}: {Response}", path, await response.Content.ReadAsStringAsync(cancellationToken));
        }

        return response.EnsureSuccessStatusCode();
    }
}