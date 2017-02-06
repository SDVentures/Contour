using System.Threading;

namespace Contour.Transport.RabbitMQ.Internal
{
    internal interface IRabbitConnectionProvider
    {
        IRabbitConnection Create();
    }
}