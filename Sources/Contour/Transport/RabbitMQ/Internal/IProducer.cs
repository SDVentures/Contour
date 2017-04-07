using System;
using System.Threading.Tasks;

namespace Contour.Transport.RabbitMQ.Internal
{
    internal interface IProducer : IDisposable
    {
        string BrokerUrl { get; }

        MessageLabel Label { get; }
        
        Task Publish(IMessage message);

        Task<IMessage> Request(IMessage request, Type expectedResponseType);

        void Start();

        void Stop();

        void UseCallbackListener(Listener listener);
    }
}