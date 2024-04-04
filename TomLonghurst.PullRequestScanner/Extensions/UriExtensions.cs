namespace TomLonghurst.PullRequestScanner.Extensions;

using System.Web;

public static class UriExtensions
{
    public static string AddQueryParam(
        this string? source, string key, string value)
    {
        string delim;
        if (source == null || !source.Contains('?'))
        {
            delim = "?";
        }
        else if (source.EndsWith("?") || source.EndsWith("&"))
        {
            delim = string.Empty;
        }
        else
        {
            delim = "&";
        }

        return source + delim + key + "=" + HttpUtility.UrlEncode(value);
    }
}