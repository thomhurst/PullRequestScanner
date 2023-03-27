using TomLonghurst.Microsoft.Extensions.DependencyInjection.ServiceInitialization;
using TomLonghurst.PullRequestScanner.Contracts;
using TomLonghurst.PullRequestScanner.Models;

namespace TomLonghurst.PullRequestScanner.Services;

internal class TeamMembersService : ITeamMembersService, IInitializer
{
    private readonly IEnumerable<ITeamMembersProvider> _teamMembersServices;

    private readonly List<TeamMember> _teamMembers = new();
    private bool _isInitialized;

    public TeamMembersService(IEnumerable<ITeamMembersProvider> teamMembersServices)
    {
        _teamMembersServices = teamMembersServices;
    }

    public async Task InitializeAsync()
    {
        if(_isInitialized)
        {
            return;
        }

        var teamMembersEnumerable = await Task.WhenAll(_teamMembersServices.Select(x => x.GetTeamMembers()));
        var teamMembers = teamMembersEnumerable.SelectMany(x => x).ToList();

        foreach (var teamMember in teamMembers)
        {
            var foundUser = FindTeamMember(teamMember);
            
            if (foundUser == null)
            {
                _teamMembers.Add(new TeamMember
                {
                    Email = teamMember.Email,
                    Ids = { teamMember.Id }, 
                    UniqueNames = { teamMember.UniqueName },
                    DisplayName = teamMember.DisplayName,
                    ImageUrls = { teamMember.ImageUrl }
                });
            }
            else
            {
                UpdateFoundUser(foundUser, teamMember);
            }
        }

        _isInitialized = true;
    }

    public TeamMember? FindTeamMember(string uniqueName)
    {
        return _teamMembers.FirstOrDefault(x => x.UniqueNames.Contains(uniqueName));
    }

    private TeamMember? FindTeamMember(ITeamMember teamMember)
    {
        return
            _teamMembers.FirstOrDefault(tm => string.Equals(tm.Email, teamMember.Email, StringComparison.InvariantCultureIgnoreCase))
            ?? _teamMembers.FirstOrDefault(tm => tm.Ids.Contains(teamMember.Id, StringComparer.InvariantCultureIgnoreCase))
            ?? _teamMembers.FirstOrDefault(tm => tm.UniqueNames.Contains(teamMember.UniqueName, StringComparer.InvariantCultureIgnoreCase))
            ?? _teamMembers.FirstOrDefault(tm => string.Equals(tm.DisplayName, teamMember.DisplayName, StringComparison.InvariantCultureIgnoreCase));
    }

    private void UpdateFoundUser(TeamMember foundUser, ITeamMember newUserDetails)
    {
        if (string.IsNullOrEmpty(foundUser.Email) && !string.IsNullOrEmpty(newUserDetails.Email))
        {
            foundUser.Email = newUserDetails.Email;
        }
        
        if (!string.IsNullOrEmpty(newUserDetails.Id))
        {
            foundUser.Ids.Add(newUserDetails.Id);
        }
        
        if (!string.IsNullOrEmpty(newUserDetails.UniqueName))
        {
            foundUser.UniqueNames.Add(newUserDetails.UniqueName);
        }
        
        if (string.IsNullOrEmpty(foundUser.DisplayName) && !string.IsNullOrEmpty(newUserDetails.DisplayName))
        {
            foundUser.DisplayName = newUserDetails.DisplayName;
        }
        
        if (!string.IsNullOrEmpty(newUserDetails.ImageUrl))
        {
            foundUser.ImageUrls.Add(newUserDetails.ImageUrl);
        }
    }

    public int Order { get; } = int.MaxValue;
}