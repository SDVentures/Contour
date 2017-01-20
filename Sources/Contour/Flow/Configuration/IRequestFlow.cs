using System;
using Contour.Flow.Execution;

namespace Contour.Flow.Configuration
{
    public interface IRequestFlow<TRequestArg>
    {
        IActingFlow<RequestContext<TRequestArg, TReply>> Request<TRequest, TReply>(string label, Func<TRequestArg, TRequest> requestor, int capacity = 1, int scale = 1);
    }
}