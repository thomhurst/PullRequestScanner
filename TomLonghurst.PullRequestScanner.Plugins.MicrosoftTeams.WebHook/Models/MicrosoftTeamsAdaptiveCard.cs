using AdaptiveCards;
using Newtonsoft.Json;

namespace TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Models
{
    internal class MicrosoftTeamsAdaptiveCard : AdaptiveCard
    {
        public MicrosoftTeamsAdaptiveCard() : base(new AdaptiveSchemaVersion(1, 3))
        {
            
        }
        
        [JsonProperty("msTeams")]
        public MicrosoftTeamsProperties MsTeams { get; set; }
    }
}