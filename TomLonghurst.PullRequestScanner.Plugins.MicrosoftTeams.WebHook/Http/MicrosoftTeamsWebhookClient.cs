namespace TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Http;

using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Extensions.Http;
using Models;
using Options;

internal class MicrosoftTeamsWebhookClient
{
    private readonly HttpClient httpClient;
    private readonly MicrosoftTeamsOptions microsoftTeamsOptions;
    private readonly ILogger<MicrosoftTeamsWebhookClient> logger;

    public MicrosoftTeamsWebhookClient(HttpClient httpClient, MicrosoftTeamsOptions microsoftTeamsOptions,
        ILogger<MicrosoftTeamsWebhookClient> logger)
    {
        this.httpClient = httpClient;
        this.microsoftTeamsOptions = microsoftTeamsOptions;
        this.logger = logger;
    }

    public async Task CreateTeamsNotification(MicrosoftTeamsAdaptiveCard adaptiveCard)
    {
        ArgumentNullException.ThrowIfNull(microsoftTeamsOptions.WebHookUri);

        var adaptiveTeamsCardJsonString = JsonConvert.SerializeObject(TeamsNotificationCardWrapper.Wrap(adaptiveCard), Formatting.None);

        logger.LogTrace("Microsoft Teams Webhook Request Payload: {Payload}", adaptiveTeamsCardJsonString);

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
                        RequestUri = microsoftTeamsOptions.WebHookUri,
                    };

                    return httpClient.SendAsync(cardsRequest);
                });

            logger.LogTrace("Microsoft Teams Webhook Response: {Response}", await teamsNotificationResponse.Content.ReadAsStringAsync());

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