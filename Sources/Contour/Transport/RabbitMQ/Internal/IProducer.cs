using System;
using System.Threading.Tasks;

namespace Contour.Transport.RabbitMQ.Internal
{
    internal interface IProducer : IDisposable
    {
        event EventHandler<ProducerStoppedEventArgs> Stopped;

        bool StopOnChannelShutdown { get; set; }

        string BrokerUrl { get; }

        string ConnectionKey { get; }

        MessageLabel Label { get; }

        Task Publish(IMessage message);

        Task<IMessage> Request(IMessage request, Type expectedResponseType);

        void Start();

        void Stop();

        void UseCallbackListener(IListener listener);

        bool IsInGoodCondition { get; }
    }
}
