using AdaptiveCards;
using Newtonsoft.Json;

namespace TomLonghurst.PullRequestScanner.Models.Teams
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