﻿namespace TomLonghurst.PullRequestScanner.GitHub.Models.GraphQl;

using System.Text.Json.Serialization;

public record CommentNode(
    [property: JsonPropertyName("author")] Author Author,
    [property: JsonPropertyName("body")] string Body,
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("updatedAt")] DateTime UpdatedAt);