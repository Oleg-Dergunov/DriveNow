using System.Net;

namespace DriveNow.Services;

public class ApiException : Exception
{
    public ApiException(string message, HttpStatusCode? statusCode = null, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }

    public HttpStatusCode? StatusCode { get; }
}