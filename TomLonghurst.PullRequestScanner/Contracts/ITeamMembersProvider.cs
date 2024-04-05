namespace TomLonghurst.PullRequestScanner.Contracts;

using Models;

public interface ITeamMembersProvider
{
    Task<IEnumerable<ITeamMember>> GetTeamMembers();
}