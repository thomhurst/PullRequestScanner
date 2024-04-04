// <copyright file="DependencyInjectionExtensions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.Extensions;

using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static PullRequestScannerBuilder AddPullRequestScanner(this IServiceCollection services)
    {
        return new PullRequestScannerBuilder(services);
    }
}