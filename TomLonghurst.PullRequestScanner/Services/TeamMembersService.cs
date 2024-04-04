namespace TomLonghurst.PullRequestScanner.Services;

using Initialization.Microsoft.Extensions.DependencyInjection;
using Contracts;
using Models;

internal class TeamMembersService : ITeamMembersService, IInitializer
{
    private readonly IEnumerable<ITeamMembersProvider> _teamMembersProviders;

    private readonly List<TeamMember> _teamMembers = new();
    private bool _isInitialized;

    public TeamMembersService(IEnumerable<ITeamMembersProvider> teamMembersProviders)
    {
        _teamMembersProviders = teamMembersProviders;
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized)
        {
            return;
        }

        var teamMembersEnumerable = await Task.WhenAll(_teamMembersProviders.Select(x => x.GetTeamMembers()));
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
                    ImageUrls = { teamMember.ImageUrl },
                });
            }
            else
            {
                UpdateFoundUser(foundUser, teamMember);
            }
        }

        _isInitialized = true;
    }

    public TeamMember? FindTeamMember(string uniqueName, string id)
    {
        return _teamMembers.FirstOrDefault(x => x.UniqueNames.Contains(uniqueName, StringComparer.InvariantCultureIgnoreCase))
        ?? _teamMembers.FirstOrDefault(x => x.Ids.Contains(id, StringComparer.InvariantCultureIgnoreCase));
    }

    private TeamMember? FindTeamMember(ITeamMember teamMember)
    {
        return
            _teamMembers.FirstOrDefault(tm => NotEmptyAndEquals(tm.Email, teamMember.Email))
            ?? _teamMembers.FirstOrDefault(tm => NotEmptyAndInList(tm.Ids, teamMember.Id))
            ?? _teamMembers.FirstOrDefault(tm => NotEmptyAndInList(tm.UniqueNames, teamMember.UniqueName))
            ?? _teamMembers.FirstOrDefault(tm => NotEmptyAndEquals(tm.DisplayName, teamMember.DisplayName));

        bool NotEmptyAndEquals(string value1, string value2)
        {
            return !string.IsNullOrEmpty(value1) && !string.IsNullOrEmpty(value2) && string.Equals(value1, value2, StringComparison.InvariantCultureIgnoreCase);
        }

        bool NotEmptyAndInList(IList<string> list, string value)
        {
            return !string.IsNullOrEmpty(value) && list.Contains(value, StringComparer.InvariantCultureIgnoreCase);
        }
    }

    private static void UpdateFoundUser(TeamMember foundUser, ITeamMember newUserDetails)
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