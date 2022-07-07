using Octokit.GraphQL;
using TomLonghurst.PullRequestScanner.Models.Github;
using TomLonghurst.PullRequestScanner.Options;

namespace TomLonghurst.PullRequestScanner.Services.Github;

internal class GithubUserService : IInitialize, IGithubUserService
{
    private readonly IGithubGraphQlClientProvider _githubGraphQlClientProvider;
    private readonly PullRequestScannerOptions _pullRequestScannerOptions;
    private GithubTeam _githubTeam;
    private bool _initialized;

    public GithubUserService(IGithubGraphQlClientProvider githubGraphQlClientProvider, PullRequestScannerOptions pullRequestScannerOptions)
    {
        _githubGraphQlClientProvider = githubGraphQlClientProvider;
        _pullRequestScannerOptions = pullRequestScannerOptions;
    }
    
    public async Task Initialize()
    {
        if (_initialized)
        {
            return;
        }

        if (!_pullRequestScannerOptions.Github.IsEnabled)
        {
            _initialized = true;
            return;
        }

        if (_pullRequestScannerOptions.Github is GithubOrganizationTeamOptions githubOrganizationTeamOptions)
        {
            await GetOrganisationTeam(githubOrganizationTeamOptions);
        }
        if (_pullRequestScannerOptions.Github is GithubUserOptions githubUserOptions)
        {
            await GetUser(githubUserOptions);
        }
        
        _initialized = true;
    }

    private async Task GetUser(GithubUserOptions githubUserOptions)
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
        
        _githubTeam = await _githubGraphQlClientProvider.GithubGraphQlClient.Run(query);
    }

    private async Task GetOrganisationTeam(GithubOrganizationTeamOptions githubOrganizationTeamOptions)
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

        _githubTeam = await _githubGraphQlClientProvider.GithubGraphQlClient.Run(query);
    }

    public List<GithubMember> GetTeamMembers()
    {
        return _githubTeam?.Members ?? new List<GithubMember>();
    }
}