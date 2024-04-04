// <copyright file="NoPullRequestPluginsRegisteredException.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.Exceptions;

public class NoPullRequestPluginsRegisteredException : PullRequestScannerException
{
    public NoPullRequestPluginsRegisteredException()
        : base("No Pull Request Plugins have been registered")
    {
    }
}