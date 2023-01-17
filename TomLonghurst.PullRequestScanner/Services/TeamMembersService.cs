using TomLonghurst.PullRequestScanner.Contracts;
using TomLonghurst.PullRequestScanner.Models;

namespace TomLonghurst.PullRequestScanner.Services;

internal class TeamMembersService : ITeamMembersService, IPullRequestScannerInitializer
{
    private readonly IEnumerable<ITeamMembersProvider> _teamMembersServices;

    private readonly List<TeamMember> _teamMembers = new();
    private bool _isInitialized;

    public TeamMembersService(IEnumerable<ITeamMembersProvider> teamMembersServices)
    {
        _teamMembersServices = teamMembersServices;
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
    
    private TeamMember? FindTeamMember(ITeamMember teamMember)
    {
        return
            FindUserWithMatchingProperty(x => x.Email, x => x.Email, teamMember)
            ?? FindUserWithPropertyContaining(x => x.Id, x => x.Ids, teamMember)
            ?? FindUserWithPropertyContaining(x => x.UniqueName, x => x.UniqueNames, teamMember)
            ?? FindUserWithMatchingProperty(x => x.DisplayName, x => x.DisplayName, teamMember);
    }
    
    public TeamMember? FindTeamMember(string uniqueName)
    {
        return _teamMembers.FirstOrDefault(x => x.UniqueNames.Contains(uniqueName));
    }

    private TeamMember? FindUserWithMatchingProperty(Func<ITeamMember, string> property1, Func<TeamMember, string> property2, ITeamMember memberDetails)
    {
        var propertyValue = property1.Invoke(memberDetails);
        
        if (string.IsNullOrEmpty(propertyValue))
        {
            return null;
        }
        
        foreach (var teamMember in _teamMembers)
        {
            var teamMemberProperty = property2.Invoke(teamMember);
            if (string.Equals(propertyValue, teamMemberProperty, StringComparison.InvariantCultureIgnoreCase))
            {
                return teamMember;
            }
        }

        return null;
    }
    
    private TeamMember? FindUserWithPropertyContaining(Func<ITeamMember, string> property1, Func<TeamMember, IEnumerable<string>> property2, ITeamMember memberDetails)
    {
        var propertyValue = property1.Invoke(memberDetails);
        
        if (string.IsNullOrEmpty(propertyValue))
        {
            return null;
        }
        
        foreach (var teamMember in _teamMembers)
        {
            var teamMemberProperty = property2.Invoke(teamMember);
            if(teamMemberProperty.Any(p => p.Contains(propertyValue, StringComparison.InvariantCultureIgnoreCase)))
            {
                return teamMember;
            }
        }

        return null;
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
}