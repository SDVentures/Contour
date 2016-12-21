using System;
using System.Runtime.Serialization;

namespace Contour.Flow.Configuration
{
    [Serializable]
    public class FlowConfigurationException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public FlowConfigurationException()
        {
        }

        public FlowConfigurationException(string message) : base(message)
        {
        }

        public FlowConfigurationException(string message, Exception inner) : base(message, inner)
        {
        }

        protected FlowConfigurationException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}