using AdaptiveCards;
using Newtonsoft.Json;
using TomLonghurst.PullRequestScanner.Models.Teams;

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

    public async Task CreateTeamsNotification(AdaptiveCard adaptiveCard)
    {
        var cardsRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            Content = new StringContent(JsonConvert.SerializeObject(TeamsNotificationCardWrapper.Wrap(adaptiveCard))),
            RequestUri = _pullRequestScannerOptions.MicrosoftTeams.WebHookUri ?? throw new ArgumentNullException(nameof(MicrosoftTeamsOptions.WebHookUri))
        };

        var teamsNotificationResponse = await _httpClient.SendAsync(cardsRequest);

        teamsNotificationResponse.EnsureSuccessStatusCode();

        var responseString = await teamsNotificationResponse.Content.ReadAsStringAsync();

        if (responseString.StartsWith("Webhook message delivery failed with error:"))
        {
            throw new HttpRequestException(responseString);
        }
    }
}