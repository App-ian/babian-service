using System.Collections.Generic;

namespace Babian.Domain.Exceptions;

public class ValidationException : BaseException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(string message) : base(message, 400)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IDictionary<string, string[]> errors) 
        : base("Une ou plusieurs erreurs de validation se sont produites.", 400)
    {
        Errors = errors;
    }
}
