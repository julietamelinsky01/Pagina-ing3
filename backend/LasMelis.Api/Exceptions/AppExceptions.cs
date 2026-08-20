namespace LasMelis.Api.Exceptions;

public class NotFoundAppException : Exception
{
    public NotFoundAppException(string message) : base(message) { }
}

public class ValidationAppException : Exception
{
    public ValidationAppException(string message) : base(message) { }
}

public class ConflictAppException : Exception
{
    public ConflictAppException(string message) : base(message) { }
}

public class UnauthorizedAppException : Exception
{
    public UnauthorizedAppException(string message) : base(message) { }
}
