using System;
using System.Runtime.Serialization;

namespace Contour.Flow.Configuration
{
    [Serializable]
    public class InvalidFlowTypeException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public InvalidFlowTypeException()
        {
        }

        public InvalidFlowTypeException(string message) : base(message)
        {
        }

        public InvalidFlowTypeException(string message, Exception inner) : base(message, inner)
        {
        }

        protected InvalidFlowTypeException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}