using System.Text;
using Newtonsoft.Json;
using Polly;
using Polly.Extensions.Http;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Models;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Options;

namespace TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Http;

internal class MicrosoftTeamsWebhookClient
{
    private readonly HttpClient _httpClient;
    private readonly MicrosoftTeamsOptions _microsoftTeamsOptions;

    public MicrosoftTeamsWebhookClient(HttpClient httpClient, MicrosoftTeamsOptions microsoftTeamsOptions)
    {
        _httpClient = httpClient;
        _microsoftTeamsOptions = microsoftTeamsOptions;
    }

    public async Task CreateTeamsNotification(MicrosoftTeamsAdaptiveCard adaptiveCard)
    {
        ArgumentNullException.ThrowIfNull(_microsoftTeamsOptions.WebHookUri);
            
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
                        RequestUri = _microsoftTeamsOptions.WebHookUri
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