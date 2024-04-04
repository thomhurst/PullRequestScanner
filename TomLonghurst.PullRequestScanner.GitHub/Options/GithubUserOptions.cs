// <copyright file="GithubUserOptions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.GitHub.Options;

public class GithubUserOptions : GithubOptions
{
    /**
     * <summary>Gets or sets the URL slug for the user.</summary>
     */
    public string Username { get; set; }

    internal override string CreateUriPathPrefix()
    {
        return $"users/{this.Username}/";
    }
}