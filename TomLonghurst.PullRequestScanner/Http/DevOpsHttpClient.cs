using System.Net.Http.Json;
using Polly;
using Polly.Extensions.Http;

namespace TomLonghurst.PullRequestScanner.Http;

internal class DevOpsHttpClient
{
    private readonly HttpClient _httpClient;

    public DevOpsHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<T?> Get<T>(string path)
    {
        var response = await 
            HttpPolicyExtensions.HandleTransientHttpError()
                .WaitAndRetryAsync(5, i => TimeSpan.FromSeconds(i * 2))
                .ExecuteAsync(() => _httpClient.GetAsync(path));

        return await response.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<T>();
    }
}