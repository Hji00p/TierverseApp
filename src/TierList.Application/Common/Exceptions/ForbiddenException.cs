namespace TierList.Application.Common.Exceptions;

public class ForbiddenException : Exception
{
    public ForbiddenException() : base("You are not authorized to perform this action.") { }
    public ForbiddenException(string message) : base(message) { }
    public ForbiddenException(string message, Exception innerException) : base(message, innerException) { }
}
