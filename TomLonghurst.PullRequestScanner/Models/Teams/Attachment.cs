using AdaptiveCards;
using Newtonsoft.Json;

namespace TomLonghurst.PullRequestScanner.Models.Teams;

internal class Attachment
{
    [JsonProperty("contentType")]
    public string ContentType { get; set; }
        
    [JsonProperty("content")]
    public AdaptiveCard Content { get; set; }
}