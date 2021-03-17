using System;
using System.Threading.Tasks;

namespace Contour.Transport.RabbitMQ.Internal
{
    internal interface IFaultTolerantProducer : IDisposable
    {
        Task<MessageExchange> Send(MessageExchange exchange, string url = null);
    }
}