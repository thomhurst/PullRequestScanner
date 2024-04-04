// <copyright file="PullRequestReviewLeaderboardModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.Models;

public class PullRequestReviewLeaderboardModel
{
    public int CommentsCount { get; set; }

    public int ReviewedCount { get; set; }
}