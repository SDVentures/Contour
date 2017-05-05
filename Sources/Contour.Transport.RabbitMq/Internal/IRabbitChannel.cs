using System;
using System.Threading;

using Contour.Receiving;
using Contour.Transport.RabbitMq.Topology;

using RabbitMQ.Client;

namespace Contour.Transport.RabbitMq.Internal
{
    internal interface IRabbitChannel : IChannel
    {
        Guid ConnectionId { get; }

        void Abort();

        void Accept(RabbitDelivery delivery);

        void Bind(Queue queue, Exchange exchange, string routingKey);

        void Reply(IMessage message, RabbitRoute replyTo, string correlationId);

        void Reject(RabbitDelivery delivery, bool requeue);

        IMessage UnpackAs(Type type, RabbitDelivery delivery);

        event Action<IChannel, ShutdownEventArgs> Shutdown;

        void SetQos(QoSParams qos);

        CancellableQueueingConsumer BuildCancellableConsumer(CancellationToken cancellationToken);

        string StartConsuming(IListeningSource listeningSource, bool requireAccept, IBasicConsumer consumer);

        ulong GetNextSeqNo();

        void EnablePublishConfirmation();

        void OnConfirmation(ConfirmationHandler handleConfirmation);

        void Publish(IRoute route, IMessage message);
    }
}