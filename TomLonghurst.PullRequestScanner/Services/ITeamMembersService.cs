// <copyright file="ITeamMembersService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.Services;

using TomLonghurst.PullRequestScanner.Models;

public interface ITeamMembersService
{
    TeamMember? FindTeamMember(string uniqueName, string id);
}