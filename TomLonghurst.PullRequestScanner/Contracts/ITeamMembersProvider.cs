// <copyright file="ITeamMembersProvider.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.Contracts;

using TomLonghurst.PullRequestScanner.Models;

public interface ITeamMembersProvider
{
    Task<IEnumerable<ITeamMember>> GetTeamMembers();
}