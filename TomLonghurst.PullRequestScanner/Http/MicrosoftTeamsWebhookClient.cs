using Newtonsoft.Json;
using Polly;
using Polly.Extensions.Http;
using TomLonghurst.PullRequestScanner.Models.Teams;
using TomLonghurst.PullRequestScanner.Options;

namespace TomLonghurst.PullRequestScanner.Http;

internal class MicrosoftTeamsWebhookClient
{
    private readonly HttpClient _httpClient;
    private readonly PullRequestScannerOptions _pullRequestScannerOptions;

    public MicrosoftTeamsWebhookClient(HttpClient httpClient, PullRequestScannerOptions pullRequestScannerOptions)
    {
        _httpClient = httpClient;
        _pullRequestScannerOptions = pullRequestScannerOptions;
    }

    public async Task CreateTeamsNotification(MicrosoftTeamsAdaptiveCard adaptiveCard)
    {
        var adaptiveTeamsCard = JsonConvert.SerializeObject(TeamsNotificationCardWrapper.Wrap(adaptiveCard));

        var teamsNotificationResponse = await HttpPolicyExtensions.HandleTransientHttpError()
            .WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(i))
            .ExecuteAsync(() =>
            {
                var cardsRequest = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    Content = new StringContent(adaptiveTeamsCard),
                    RequestUri = _pullRequestScannerOptions.MicrosoftTeams.WebHookUri ??
                                 throw new ArgumentNullException(nameof(MicrosoftTeamsOptions.WebHookUri))
                };

                return _httpClient.SendAsync(cardsRequest);
            });

        var responseString = await teamsNotificationResponse.Content.ReadAsStringAsync();

        if ((responseString.StartsWith("Webhook message delivery failed") || !teamsNotificationResponse.IsSuccessStatusCode)
            && adaptiveTeamsCard.Length > 28000)
        {
            throw new HttpRequestException($"Teams card payload is too big - {adaptiveTeamsCard.Length} bytes");
        }
        
        if (responseString.StartsWith("Webhook message delivery failed"))
        {
            throw new HttpRequestException(responseString, null, teamsNotificationResponse.StatusCode);
        }
        
        teamsNotificationResponse.EnsureSuccessStatusCode();
    }
}