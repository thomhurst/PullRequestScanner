using System.Net.Http.Json;
using Polly;
using Polly.Extensions.Http;

namespace TomLonghurst.PullRequestScanner.Http;

internal class GithubHttpClient
{
    private readonly HttpClient _client;

    public GithubHttpClient(HttpClient httpClient)
    {
        _client = httpClient;
    }
    
    public async Task<T?> Get<T>(string path)
    {
        var response = await 
            HttpPolicyExtensions.HandleTransientHttpError()
                .WaitAndRetryAsync(5, i => TimeSpan.FromSeconds(i * 2))
                .ExecuteAsync(() => _client.GetAsync(path));

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine(await response.Content.ReadAsStringAsync());
        }

        return await response.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<T>();
    }
}