using System.Text.Json.Serialization;

namespace TomLonghurst.PullRequestScanner.Models.DevOps;

public record DevOpsTeamMember(
    [property: JsonPropertyName("identity")] Identity Identity,
    [property: JsonPropertyName("isTeamAdmin")] bool? IsTeamAdmin
);