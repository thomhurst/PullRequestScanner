namespace TomLonghurst.PullRequestScanner.Contracts;

using TomLonghurst.PullRequestScanner.Models;

public interface ITeamMembersProvider
{
    Task<IEnumerable<ITeamMember>> GetTeamMembers();
}