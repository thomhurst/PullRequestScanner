using System.Diagnostics;
using System.Net.Http.Json;

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
        var response = await _httpClient.GetAsync(path);

        return await response.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<T>();
    }
    
    public async Task<string> GetString(string path)
    {
        var response = await _httpClient.GetAsync(path);

        if (!response.IsSuccessStatusCode)
        {
            Debugger.Break();
        }

        return await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
    }
}