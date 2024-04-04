using TomLonghurst.PullRequestScanner.Models;

namespace TomLonghurst.PullRequestScanner.Contracts;

public interface ITeamMembersProvider
{
    Task<IEnumerable<ITeamMember>> GetTeamMembers();
}