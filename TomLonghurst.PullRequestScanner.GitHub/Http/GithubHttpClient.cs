namespace TomLonghurst.PullRequestScanner.GitHub.Http;

using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;

internal class GithubHttpClient
{
    private readonly HttpClient _client;
    private readonly ILogger<GithubHttpClient> _logger;

    public GithubHttpClient(
        HttpClient httpClient,
        ILogger<GithubHttpClient> logger)
    {
        _client = httpClient;
        this._logger = logger;
    }

    public async Task<T?> Get<T>(string path)
    {
        var response = await
            HttpPolicyExtensions.HandleTransientHttpError()
                .WaitAndRetryAsync(5, i => TimeSpan.FromSeconds(i * 2))
                .ExecuteAsync(() => _client.GetAsync(path));

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Error calling {Path}: {Response}", path, await response.Content.ReadAsStringAsync());
        }

        return await response.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<T>();
    }
}