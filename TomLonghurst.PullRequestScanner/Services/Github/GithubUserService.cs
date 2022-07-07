using Octokit.GraphQL;
using TomLonghurst.PullRequestScanner.Models.Github;
using TomLonghurst.PullRequestScanner.Options;

namespace TomLonghurst.PullRequestScanner.Services.Github;

internal class GithubUserService : IGithubUserService
{
    private readonly IGithubGraphQlClientProvider _githubGraphQlClientProvider;
    private readonly PullRequestScannerOptions _pullRequestScannerOptions;

    public GithubUserService(IGithubGraphQlClientProvider githubGraphQlClientProvider, PullRequestScannerOptions pullRequestScannerOptions)
    {
        _githubGraphQlClientProvider = githubGraphQlClientProvider;
        _pullRequestScannerOptions = pullRequestScannerOptions;
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
                        Email = x.Email
                    }
                },
                Slug = x.Login
            })
            .Compile();
        
        return await _githubGraphQlClientProvider.GithubGraphQlClient.Run(query);
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
                        Id = m.Id.Value
                    }).ToList()
            }).Compile();

        return await _githubGraphQlClientProvider.GithubGraphQlClient.Run(query);
    }

    public async Task<IReadOnlyList<GithubMember>> GetTeamMembers()
    {
        if (!_pullRequestScannerOptions.Github.IsEnabled)
        {
            return new List<GithubMember>();
        }

        if (_pullRequestScannerOptions.Github is GithubOrganizationTeamOptions githubOrganizationTeamOptions)
        {
            var team = await GetOrganisationTeam(githubOrganizationTeamOptions);
            return team.Members;
        }
        if (_pullRequestScannerOptions.Github is GithubUserOptions githubUserOptions)
        {
            var team = await GetUser(githubUserOptions);
            return team.Members;
        }
        
        return new List<GithubMember>();
    }
}