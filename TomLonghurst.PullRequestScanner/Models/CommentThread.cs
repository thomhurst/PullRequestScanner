// <copyright file="CommentThread.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.Models;

public record CommentThread
{
    public PullRequest ParentPullRequest { get; set; }

    public List<Comment> Comments { get; set; } = new();

    public ThreadStatus Status { get; set; }
}