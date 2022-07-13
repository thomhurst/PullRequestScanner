using TomLonghurst.PullRequestScanner.Models.DevOps;
using TomLonghurst.PullRequestScanner.Models.Github;
using TomLonghurst.PullRequestScanner.Services.DevOps;
using TomLonghurst.PullRequestScanner.Services.Github;

namespace TomLonghurst.PullRequestScanner.Services;

internal class TeamMembersService : ITeamMembersService, IInitialize
{
    private readonly IDevOpsUserService _devOpsUserService;
    private readonly IGithubUserService _githubUserService;

    private readonly List<TeamMember> _teamMembers = new();
    private bool _isInitialized;

    public TeamMembersService(IDevOpsUserService devOpsUserService, 
        IGithubUserService githubUserService)
    {
        _devOpsUserService = devOpsUserService;
        _githubUserService = githubUserService;
    }

    public IReadOnlyList<TeamMember> GetTeamMembers()
    {
        return _teamMembers;
    }

    public async Task Initialize()
    {
        if (_isInitialized)
        {
            return;
        }
        
        var githubUsersTask = _githubUserService.GetTeamMembers();
        var devopsUsersTask = _devOpsUserService.GetTeamMembers();
        
        var githubUsers = await githubUsersTask;
        var devopsUsers = await devopsUsersTask;
        
        foreach (var githubUser in githubUsers)
        {
            var foundUser = FindGithubTeamMember(githubUser);
            if (foundUser == null)
            {
                _teamMembers.Add(new TeamMember
                {
                    Email = githubUser.Email,
                    GithubId = githubUser.Id,
                    GithubUsername = githubUser.UniqueName,
                    DisplayName = githubUser.DisplayName,
                    GithubImageUrl = githubUser.ImageUrl
                });
            }
            else
            {
                UpdateFoundUser(foundUser, githubUser);
            }
        }
        
        foreach (var devOpsUser in devopsUsers)
        {
            var foundUser = FindDevOpsTeamMember(devOpsUser);
            if (foundUser == null)
            {
                _teamMembers.Add(new TeamMember
                {
                    Email = devOpsUser.Identity.UniqueName,
                    DevOpsId = devOpsUser.Identity.Id,
                    DevOpsUsername = devOpsUser.Identity.UniqueName,
                    DisplayName = devOpsUser.Identity.DisplayName,
                    DevOpsImageUrl = devOpsUser.Identity.ImageUrl
                });
            }
            else
            {
                UpdateFoundUser(foundUser, devOpsUser);
            }
        }

        _isInitialized = true;
    }
    
    private TeamMember? FindGithubTeamMember(GithubMember githubUser)
    {
        return
            FindUserWithMatchingProperty(x => x.Email, x => x.Email, githubUser)
            ?? FindUserWithMatchingProperty(x => x.Id, x => x.GithubId, githubUser)
            ?? FindUserWithMatchingProperty(x => x.UniqueName, x => x.GithubUsername, githubUser)
            ?? FindUserWithMatchingProperty(x => x.DisplayName, x => x.DisplayName, githubUser);
    }
    
    public TeamMember? FindGithubTeamMember(string githubUsername)
    {
        return _teamMembers.FirstOrDefault(x => x.GithubUsername == githubUsername);
    }

    public TeamMember? FindDevOpsTeamMember(string devOpsUsername)
    {
        return _teamMembers.FirstOrDefault(x => x.DevOpsUsername == devOpsUsername);
    }

    private TeamMember? FindUserWithMatchingProperty(Func<GithubMember, string> property1, Func<TeamMember, string> property2, GithubMember githubMember)
    {
        var githubProperty = property1.Invoke(githubMember);
        
        if (string.IsNullOrEmpty(githubProperty))
        {
            return null;
        }
        
        foreach (var teamMember in _teamMembers)
        {
            var teamMemberProperty = property2.Invoke(teamMember);
            if (string.Equals(githubProperty, teamMemberProperty, StringComparison.InvariantCultureIgnoreCase))
            {
                return teamMember;
            }
        }

        return null;
    }

    private void UpdateFoundUser(TeamMember foundUser, GithubMember githubUser)
    {
        if (string.IsNullOrEmpty(foundUser.Email) && !string.IsNullOrEmpty(githubUser.Email))
        {
            foundUser.Email = githubUser.Email;
        }
        
        if (string.IsNullOrEmpty(foundUser.GithubId) && !string.IsNullOrEmpty(githubUser.Id))
        {
            foundUser.GithubId = githubUser.Id;
        }
        
        if (string.IsNullOrEmpty(foundUser.GithubUsername) && !string.IsNullOrEmpty(githubUser.UniqueName))
        {
            foundUser.GithubUsername = githubUser.UniqueName;
        }
        
        if (string.IsNullOrEmpty(foundUser.DisplayName) && !string.IsNullOrEmpty(githubUser.DisplayName))
        {
            foundUser.DisplayName = githubUser.DisplayName;
        }
        
        if (string.IsNullOrEmpty(foundUser.GithubImageUrl) && !string.IsNullOrEmpty(githubUser.ImageUrl))
        {
            foundUser.GithubImageUrl = githubUser.ImageUrl;
        }
    }

    private TeamMember? FindDevOpsTeamMember(DevOpsTeamMember devOpsTeamMember)
    {
        return
            FindUserWithMatchingProperty(x => x.Identity.UniqueName, x => x.Email, devOpsTeamMember)
            ?? FindUserWithMatchingProperty(x => x.Identity.Id, x => x.GithubId, devOpsTeamMember)
            ?? FindUserWithMatchingProperty(x => x.Identity.UniqueName, x => x.GithubUsername, devOpsTeamMember)
            ?? FindUserWithMatchingProperty(x => x.Identity.DisplayName, x => x.DisplayName, devOpsTeamMember);
    }

    private TeamMember? FindUserWithMatchingProperty(Func<DevOpsTeamMember, string> property1, Func<TeamMember, string> property2, DevOpsTeamMember devOpsTeamMember)
    {
        var devOpsProperty = property1.Invoke(devOpsTeamMember);
        
        if (string.IsNullOrEmpty(devOpsProperty))
        {
            return null;
        }
        
        foreach (var teamMember in _teamMembers)
        {
            var teamMemberProperty = property2.Invoke(teamMember);
            if (string.Equals(devOpsProperty, teamMemberProperty, StringComparison.InvariantCultureIgnoreCase))
            {
                return teamMember;
            }
        }

        return null;
    }

    private void UpdateFoundUser(TeamMember foundUser, DevOpsTeamMember devOpsTeamMember)
    {
        if (!string.IsNullOrEmpty(devOpsTeamMember.Identity.UniqueName))
        {
            foundUser.Email = devOpsTeamMember.Identity.UniqueName;
        }
        
        if (string.IsNullOrEmpty(foundUser.DevOpsId) && !string.IsNullOrEmpty(devOpsTeamMember.Identity.Id))
        {
            foundUser.DevOpsId = devOpsTeamMember.Identity.Id;
        }
        
        if (string.IsNullOrEmpty(foundUser.DevOpsUsername) && !string.IsNullOrEmpty(devOpsTeamMember.Identity.UniqueName))
        {
            foundUser.DevOpsUsername = devOpsTeamMember.Identity.UniqueName;
        }
        
        if (string.IsNullOrEmpty(foundUser.DisplayName) && !string.IsNullOrEmpty(devOpsTeamMember.Identity.DisplayName))
        {
            foundUser.DisplayName = devOpsTeamMember.Identity.DisplayName;
        }
        
        if (string.IsNullOrEmpty(foundUser.DevOpsImageUrl) && !string.IsNullOrEmpty(devOpsTeamMember.Identity.ImageUrl))
        {
            foundUser.DevOpsImageUrl = devOpsTeamMember.Identity.ImageUrl;
        }
    }
}