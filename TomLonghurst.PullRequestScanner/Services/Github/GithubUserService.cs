using Octokit.GraphQL;
using TomLonghurst.PullRequestScanner.Models.Github;

namespace TomLonghurst.PullRequestScanner.Services.Github;

internal class GithubUserService : IInitialize, IGithubUserService
{
    private readonly IGithubGraphQlClientProvider _githubGraphQlClientProvider;
    private GithubTeam _githubTeam;
    private bool _initialized;

    public GithubUserService(IGithubGraphQlClientProvider githubGraphQlClientProvider)
    {
        _githubGraphQlClientProvider = githubGraphQlClientProvider;
    }
    
    public async Task Initialize()
    {
        if (_initialized)
        {
            return;
        }
        
        var query = new Query()
            .Organization("asosteam")
            .Team("customer-profile-identity")
            .Select(x => new GithubTeam
            {
                Name = x.Name,
                Id = x.Id.Value,
                Slug = x.Slug,
                Members = x.Members(null, null, null, null, null, null, null, null).AllPages().Select(m =>
                    new GithubMember
                    {
                        DisplayName = m.Name,
                        UniqueName = m.Login,
                        Id = m.Id.Value
                    }).ToList()
            }).Compile();

        _githubTeam = await _githubGraphQlClientProvider.GithubGraphQlClient.Run(query);
        _initialized = true;
    }

    public GithubTeam GetTeam()
    {
        return _githubTeam;
    }
}