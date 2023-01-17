using System.Text.Json.Serialization;
using TomLonghurst.PullRequestScanner.Models;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Models;

public record DevOpsTeamMember(
    [property: JsonPropertyName("identity")] Identity Identity,
    [property: JsonPropertyName("isTeamAdmin")] bool? IsTeamAdmin
) : ITeamMember
{
    public string DisplayName { get; } = Identity?.DisplayName;
    public string UniqueName { get; } = Identity?.UniqueName;
    public string Id { get; } = Identity?.Id;
    public string Email { get; } = Identity?.UniqueName;
    public string ImageUrl { get; } = Identity?.ImageUrl;
}