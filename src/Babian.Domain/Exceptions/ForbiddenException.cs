namespace Babian.Domain.Exceptions;

public class ForbiddenException : BaseException
{
    public ForbiddenException(string message = "Accès refusé à cette ressource.") : base(message, 403)
    {
    }
}
