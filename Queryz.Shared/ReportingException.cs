using System;
using System.Runtime.Serialization;

namespace Queryz
{
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

        protected ReportingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}