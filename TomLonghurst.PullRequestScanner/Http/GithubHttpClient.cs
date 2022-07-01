using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;

namespace TomLonghurst.PullRequestScanner.Http;

internal class GithubHttpClient
{
    public HttpClient Client { get; }

    public GithubHttpClient(HttpClient httpClient)
    {
        Client = httpClient;
    }
    
    public async Task<T?> Get<T>(string path)
    {
        var response = await Client.GetAsync(path);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine(await response.Content.ReadAsStringAsync());
        }

        return await response.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<T>();
    }
    
    public async Task<string> GetString(string path)
    {
        var response = await Client.GetAsync(path);

        if (!response.IsSuccessStatusCode)
        {
            Debugger.Break();
        }

        return await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
    }
    
    public async Task<T?> Post<T>(string path, object body)
    {
        var response = await Client.PostAsync(path, new StringContent(JsonSerializer.Serialize(body)));

        return await response.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<T>();
    }
    
    public async Task<string> PostString(string path, object body)
    {
        var response = await Client.PostAsync(path, new StringContent(JsonSerializer.Serialize(body)));

        if (!response.IsSuccessStatusCode)
        {
            Debugger.Break();
        }

        return await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
    }
}