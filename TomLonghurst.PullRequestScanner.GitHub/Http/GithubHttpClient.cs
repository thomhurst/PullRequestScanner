namespace TomLonghurst.PullRequestScanner.GitHub.Http;

using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;

internal class GithubHttpClient
{
    private readonly HttpClient client;
    private readonly ILogger<GithubHttpClient> logger;

    public GithubHttpClient(
        HttpClient httpClient,
        ILogger<GithubHttpClient> logger)
    {
        client = httpClient;
        this.logger = logger;
    }

    public async Task<T?> Get<T>(string path)
    {
        var response = await
            HttpPolicyExtensions.HandleTransientHttpError()
                .WaitAndRetryAsync(5, i => TimeSpan.FromSeconds(i * 2))
                .ExecuteAsync(() => client.GetAsync(path));

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Error calling {Path}: {Response}", path, await response.Content.ReadAsStringAsync());
        }

        return await response.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<T>();
    }
}