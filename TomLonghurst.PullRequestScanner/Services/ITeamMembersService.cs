namespace TomLonghurst.PullRequestScanner.Services;

internal interface ITeamMembersService
{
    IReadOnlyList<TeamMember> GetTeamMembers();
    TeamMember? FindGithubTeamMember(string githubUsername);
    TeamMember? FindDevOpsTeamMember(string devOpsUsername);
}