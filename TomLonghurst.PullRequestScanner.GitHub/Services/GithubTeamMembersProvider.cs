using Octokit.GraphQL;
using TomLonghurst.PullRequestScanner.Contracts;
using TomLonghurst.PullRequestScanner.GitHub.Models;
using TomLonghurst.PullRequestScanner.GitHub.Options;
using TomLonghurst.PullRequestScanner.Models;

namespace TomLonghurst.PullRequestScanner.GitHub.Services;

internal class GithubTeamMembersProvider : ITeamMembersProvider
{
    private readonly IGithubQueryRunner _githubQueryRunner;
    private readonly GithubOptions _githubOptions;

    public GithubTeamMembersProvider(IGithubQueryRunner githubQueryRunner, GithubOptions githubOptions)
    {
        _githubQueryRunner = githubQueryRunner;
        _githubOptions = githubOptions;
    }

    private async Task<GithubTeam> GetUser(GithubUserOptions githubUserOptions)
    {
        var query = new Query()
            .User(githubUserOptions.Username)
            .Select(x => new GithubTeam
            {
                Name = x.Login,
                Id = x.Id.Value,
                Members = new List<GithubMember>
                {
                    new()
                    {
                        Id = x.Id.Value,
                        DisplayName = x.Name ?? x.Login,
                        UniqueName = x.Login,
                        Email = x.Email,
                        ImageUrl = x.AvatarUrl(null)
                    }
                },
                Slug = x.Login
            })
            .Compile();
        
        return await _githubQueryRunner.RunQuery(query);
    }

    private async Task<GithubTeam> GetOrganisationTeam(GithubOrganizationTeamOptions githubOrganizationTeamOptions)
    {
        var query = new Query()
            .Organization(githubOrganizationTeamOptions.OrganizationSlug)
            .Team(githubOrganizationTeamOptions.TeamSlug)
            .Select(x => new GithubTeam
            {
                Name = x.Name,
                Id = x.Id.Value,
                Slug = x.Slug,
                Members = x.Members(null, null, null, null, null, null, null, null)
                    .AllPages()
                    .Select(m =>
                    new GithubMember
                    {
                        DisplayName = m.Name,
                        UniqueName = m.Login,
                        Id = m.Id.Value,
                        Email = m.Email,
                        ImageUrl = m.AvatarUrl(null)
                    }).ToList()
            }).Compile();

        return await _githubQueryRunner.RunQuery(query);
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