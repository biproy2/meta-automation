namespace Ecommerce.Application.Common.Exceptions;

/// <summary>Thrown when a DB record is not found. Middleware maps this to HTTP 404.</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string entity, object key)
        : base($"'{entity}' with key '{key}' was not found.") { }

    public NotFoundException(string message) : base(message) { }
}
