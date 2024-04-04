using Octokit;
using TomLonghurst.PullRequestScanner.Contracts;
using TomLonghurst.PullRequestScanner.GitHub.Models;
using TomLonghurst.PullRequestScanner.GitHub.Options;
using TomLonghurst.PullRequestScanner.Models;

namespace TomLonghurst.PullRequestScanner.GitHub.Services;

internal class GithubTeamMembersProvider : ITeamMembersProvider
{
    private readonly IGitHubClient _gitHubClient;
    private readonly GithubOptions _githubOptions;

    public GithubTeamMembersProvider(IGitHubClient gitHubClient, GithubOptions githubOptions)
    {
        _gitHubClient = gitHubClient;
        _githubOptions = githubOptions;
    }

    private async Task<GithubTeam> GetUser(GithubUserOptions githubUserOptions)
    {
        var user = await _gitHubClient.User.Get(githubUserOptions.Username);

        return new GithubTeam
        {
            Name = user.Login,
            Id = user.Id.ToString(),
            Members = new List<GithubMember>
            {
                new()
                {
                    Id = user.Id.ToString(),
                    DisplayName = user.Name ?? user.Login,
                    UniqueName = user.Login,
                    Email = user.Email,
                    ImageUrl = user.AvatarUrl
                }
            },
            Slug = user.Login
        };
    }

    private async Task<GithubTeam> GetOrganisationTeam(GithubOrganizationTeamOptions githubOrganizationTeamOptions)
    {
        var team = await _gitHubClient.Organization.Team.GetByName(githubOrganizationTeamOptions.OrganizationSlug,
            githubOrganizationTeamOptions.TeamSlug);

        var members = await _gitHubClient
            .Organization
            .Team
            .GetAllMembers(team.Id);

        return new GithubTeam
        {
            Name = team.Name,
            Id = team.Id.ToString(),
            Slug = team.Slug,
            Members = members
                    .Select(m =>
                    new GithubMember
                    {
                        DisplayName = m.Name,
                        UniqueName = m.Login,
                        Id = m.Id.ToString(),
                        Email = m.Email,
                        ImageUrl = m.AvatarUrl
                    }).ToList()
        };
    }

    public async Task<IEnumerable<ITeamMember>> GetTeamMembers()
    {
        if (!_githubOptions.IsEnabled)
        {
            return Array.Empty<ITeamMember>();
        }

        if (_githubOptions is GithubOrganizationTeamOptions githubOrganizationTeamOptions)
        {
            var team = await GetOrganisationTeam(githubOrganizationTeamOptions);
            return team.Members;
        }

        if (_githubOptions is GithubUserOptions githubUserOptions)
        {
            var team = await GetUser(githubUserOptions);
            return team.Members;
        }

        return new List<GithubMember>();
    }
}