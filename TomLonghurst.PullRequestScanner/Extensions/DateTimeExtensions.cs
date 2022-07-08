namespace TomLonghurst.PullRequestScanner.Extensions;

public static class DateTimeExtensions
{
    public static bool IsYesterday(this DateTimeOffset? dateTimeOffset)
    {
        if (dateTimeOffset is null)
        {
            return false;
        }

        return IsYesterday(dateTimeOffset.Value);
    }
    
    public static bool IsYesterday(this DateTimeOffset dateTimeOffset)
    {
        var today = DateTimeOffset.UtcNow.Date;
        var yesterday = today.AddDays(-1);

        return dateTimeOffset > yesterday && dateTimeOffset < today;
    }
}