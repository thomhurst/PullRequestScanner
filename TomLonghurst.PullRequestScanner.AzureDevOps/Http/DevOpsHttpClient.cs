using System.Diagnostics;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using TomLonghurst.PullRequestScanner.Extensions;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Http;

internal class DevOpsHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DevOpsHttpClient> _logger;

    public DevOpsHttpClient(HttpClient httpClient, ILogger<DevOpsHttpClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    
    public async Task<T?> Get<T>(string path)
    {
        var httpResponseMessage = await GetHttpResponseMessage(path);

        return await httpResponseMessage.Content.ReadFromJsonAsync<T>();
    }

    public async Task<IEnumerable<T>> GetAll<T>(string path) where T : IHasCount
    {
        var results = new List<T>();

        var iteration = 0;
        do
        {
            Console.WriteLine("Memory Used:" + Process.GetCurrentProcess().PrivateMemorySize64);
            
            var pathWithBatchingQueryParameters = path
                .AddQueryParam("$top", "100")
                .AddQueryParam("$skip", (100 * iteration).ToString());

            var response = await Get<T>(pathWithBatchingQueryParameters);

            results.Add(response);
            
            iteration++;
        } while (results.Last().Count == 100 && (100*iteration) == results.Sum(r => r.Count));

        return results;
    }

    private async Task<HttpResponseMessage> GetHttpResponseMessage(string path)
    {
        var response = await
            HttpPolicyExtensions.HandleTransientHttpError()
                .WaitAndRetryAsync(5, i => TimeSpan.FromSeconds(i * 2))
                .ExecuteAsync(() => _httpClient.GetAsync(path));

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Error calling {Path}: {Response}", path, await response.Content.ReadAsStringAsync());
        }

        return response.EnsureSuccessStatusCode();
    }
}