namespace Queryz;

public class ReportingException : ApplicationException
{
    public ReportingException()
    {
    }

    public ReportingException(string message)
        : base(message)
    {
    }

    public ReportingException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}