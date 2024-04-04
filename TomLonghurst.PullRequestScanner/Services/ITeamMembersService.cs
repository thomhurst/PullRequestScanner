namespace TomLonghurst.PullRequestScanner.Services;

using TomLonghurst.PullRequestScanner.Models;

public interface ITeamMembersService
{
    TeamMember? FindTeamMember(string uniqueName, string id);
}