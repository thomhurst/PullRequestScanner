// <copyright file="Approver.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.Models;

public record Approver
{
    public TeamMember TeamMember { get; set; }

    public bool IsRequired { get; set; }

    public Vote Vote { get; set; }

    public DateTimeOffset? Time { get; set; }

    public PullRequest PullRequest { get; set; }
}