namespace Ecommerce.Application.Common.Exceptions;

/// <summary>Thrown when WhatsApp, Messenger, or Pathao API returns an error.</summary>
public class ExternalApiException : Exception
{
    public string ApiName { get; }
    public int? StatusCode { get; }

    public ExternalApiException(string apiName, string message, int? statusCode = null)
        : base($"[{apiName}] {message}")
    {
        ApiName = apiName;
        StatusCode = statusCode;
    }
}
