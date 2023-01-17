using TomLonghurst.PullRequestScanner.Models;

namespace TomLonghurst.PullRequestScanner.Services;

public interface ITeamMembersService
{
    IReadOnlyList<TeamMember> 
        GetTeamMembers();
    TeamMember? FindTeamMember(string uniqueName);
}