using System.Text;
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
        ArgumentNullException.ThrowIfNull(_pullRequestScannerOptions.MicrosoftTeams.WebHookUri);
            
        var adaptiveTeamsCardJsonString = JsonConvert.SerializeObject(TeamsNotificationCardWrapper.Wrap(adaptiveCard), Formatting.None);

        try
        {
            var teamsNotificationResponse = await HttpPolicyExtensions.HandleTransientHttpError()
                .WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(i))
                .ExecuteAsync(() =>
                {
                    var cardsRequest = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        Content = new StringContent(adaptiveTeamsCardJsonString),
                        RequestUri = _pullRequestScannerOptions.MicrosoftTeams.WebHookUri
                    };

                    return _httpClient.SendAsync(cardsRequest);
                });

            teamsNotificationResponse.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException e)
        {
            if (e.Message.StartsWith("Webhook message delivery failed with error: Microsoft Teams endpoint returned HTTP error 413"))
            {
                var byteCount = Encoding.Unicode.GetByteCount(adaptiveTeamsCardJsonString);
                throw new HttpRequestException($"Teams card payload is too big - {byteCount} bytes", e);
            }
            
            throw;
        }
    }
}