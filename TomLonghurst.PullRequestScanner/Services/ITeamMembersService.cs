using TomLonghurst.PullRequestScanner.Models;

namespace TomLonghurst.PullRequestScanner.Services;

public interface ITeamMembersService
{
    TeamMember? FindTeamMember(string uniqueName, string id);
}