using System.Runtime.Serialization;

namespace TomLonghurst.PullRequestScanner.Exceptions;

public class PullRequestScannerException : Exception
{
    public PullRequestScannerException()
    {
    }

    protected PullRequestScannerException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public PullRequestScannerException(string? message) : base(message)
    {
    }

    public PullRequestScannerException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}