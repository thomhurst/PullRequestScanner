namespace TomLonghurst.PullRequestScanner.Services;

using Initialization.Microsoft.Extensions.DependencyInjection;
using TomLonghurst.PullRequestScanner.Contracts;
using TomLonghurst.PullRequestScanner.Models;

internal class TeamMembersService : ITeamMembersService, IInitializer
{
    private readonly IEnumerable<ITeamMembersProvider> teamMembersProviders;

    private readonly List<TeamMember> teamMembers = new();
    private bool isInitialized;

    public TeamMembersService(IEnumerable<ITeamMembersProvider> teamMembersProviders)
    {
        this.teamMembersProviders = teamMembersProviders;
    }

    public async Task InitializeAsync()
    {
        if (this.isInitialized)
        {
            return;
        }

        var teamMembersEnumerable = await Task.WhenAll(this.teamMembersProviders.Select(x => x.GetTeamMembers()));
        var teamMembers = teamMembersEnumerable.SelectMany(x => x).ToList();

        foreach (var teamMember in teamMembers)
        {
            var foundUser = this.FindTeamMember(teamMember);

            if (foundUser == null)
            {
                this.teamMembers.Add(new TeamMember
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

        this.isInitialized = true;
    }

    public TeamMember? FindTeamMember(string uniqueName, string id)
    {
        return this.teamMembers.FirstOrDefault(x => x.UniqueNames.Contains(uniqueName, StringComparer.InvariantCultureIgnoreCase))
        ?? this.teamMembers.FirstOrDefault(x => x.Ids.Contains(id, StringComparer.InvariantCultureIgnoreCase));
    }

    private TeamMember? FindTeamMember(ITeamMember teamMember)
    {
        return
            this.teamMembers.FirstOrDefault(tm => NotEmptyAndEquals(tm.Email, teamMember.Email))
            ?? this.teamMembers.FirstOrDefault(tm => NotEmptyAndInList(tm.Ids, teamMember.Id))
            ?? this.teamMembers.FirstOrDefault(tm => NotEmptyAndInList(tm.UniqueNames, teamMember.UniqueName))
            ?? this.teamMembers.FirstOrDefault(tm => NotEmptyAndEquals(tm.DisplayName, teamMember.DisplayName));

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