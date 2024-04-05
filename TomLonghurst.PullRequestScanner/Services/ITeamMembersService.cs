namespace TomLonghurst.PullRequestScanner.Services;

using Models;

public interface ITeamMembersService
{
    TeamMember? FindTeamMember(string uniqueName, string id);
}