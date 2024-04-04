// <copyright file="GithubMember.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.GitHub.Models;

using TomLonghurst.PullRequestScanner.Models;

internal class GithubMember : ITeamMember
{
    public string DisplayName { get; set; }

    public string UniqueName { get; set; }

    public string Id { get; set; }

    public string Email { get; set; }

    public string ImageUrl { get; set; }
}