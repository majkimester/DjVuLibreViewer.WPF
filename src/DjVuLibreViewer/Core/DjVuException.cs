using System;
using System.Runtime.Serialization;


namespace DjVuLibreViewer.Core
{
    public class DjVuException : Exception
    {
        public DjVuException()
        {
        }

        public DjVuException(string message)
            : base(message)
        {
        }

        public DjVuException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected DjVuException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
