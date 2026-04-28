using FluentValidation.Results;
namespace Ecommerce.Application.Common.Exceptions;
public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }
    public ValidationException() : base("Validation errors occurred.") => Errors = new Dictionary<string, string[]>();
    public ValidationException(IEnumerable<ValidationFailure> failures) : this()
    {
        Errors = failures.GroupBy(f => f.PropertyName, f => f.ErrorMessage).ToDictionary(g => g.Key, g => g.ToArray());
    }
}
