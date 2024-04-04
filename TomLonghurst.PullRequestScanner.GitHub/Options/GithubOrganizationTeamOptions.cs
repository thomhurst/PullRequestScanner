// <copyright file="GithubOrganizationTeamOptions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.GitHub.Options;

public class GithubOrganizationTeamOptions : GithubOptions
{
    /**
     * <summary>Gets or sets the URL slug for the organization.</summary>
     */
    public string OrganizationSlug { get; set; }
    /**
     * <summary>Gets or sets the URL slug for the team.</summary>
     */
    public string TeamSlug { get; set; }

    internal override string CreateUriPathPrefix()
    {
        return $"orgs/{this.OrganizationSlug}/teams/{this.TeamSlug}/";
    }
}