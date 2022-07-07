using TomLonghurst.PullRequestScanner.Models.DevOps;

namespace TomLonghurst.PullRequestScanner.Services.DevOps;

internal interface IDevOpsUserService
{
    Task<IReadOnlyList<DevOpsTeamMember>> GetTeamMembers();    
}