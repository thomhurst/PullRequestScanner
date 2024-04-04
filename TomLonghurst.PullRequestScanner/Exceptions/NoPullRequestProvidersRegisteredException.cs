// <copyright file="NoPullRequestProvidersRegisteredException.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.Exceptions;

public class NoPullRequestProvidersRegisteredException : PullRequestScannerException
{
    public NoPullRequestProvidersRegisteredException()
        : base("No Pull Request Providers have been registered")
    {
    }
}