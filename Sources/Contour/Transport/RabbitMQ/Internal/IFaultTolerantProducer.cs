using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Contour.Transport.RabbitMQ.Internal
{
    internal interface IFaultTolerantProducer : IDisposable
    {
        IEnumerable<KeyValuePair<int, int>> Delays { get; }
        
        Task<MessageExchange> Send(MessageExchange exchange);
    }
}