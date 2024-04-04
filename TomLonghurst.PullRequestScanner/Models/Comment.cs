// <copyright file="Comment.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.Models;

public record Comment
{
    public CommentThread ParentCommentThread { get; set; }

    public TeamMember Author { get; set; }

    public DateTimeOffset LastUpdated { get; set; }
}