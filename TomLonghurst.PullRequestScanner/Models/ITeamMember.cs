// <copyright file="ITeamMember.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.Models;

public interface ITeamMember
{
    string DisplayName { get; }

    string UniqueName { get; }

    string Id { get; }

    string Email { get; }

    string ImageUrl { get; }
}