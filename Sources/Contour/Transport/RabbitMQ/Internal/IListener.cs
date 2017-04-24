using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Contour.Receiving;
using Contour.Receiving.Consumers;
using Contour.Validation;
using RabbitMQ.Client.Events;

namespace Contour.Transport.RabbitMQ.Internal
{
    internal interface IListener : IDisposable
    {
        event EventHandler<ListenerStoppedEventArgs> Stopped;

        bool StopOnChannelShutdown { get; set; }

        IEnumerable<MessageLabel> AcceptedLabels { get; }

        string BrokerUrl { get; }

        ISubscriptionEndpoint Endpoint { get; }

        RabbitReceiverOptions ReceiverOptions { get; }

        RabbitDelivery BuildDeliveryFrom(RabbitChannel deliveryChannel, BasicDeliverEventArgs args);
        
        Task<IMessage> Expect(string correlationId, Type expectedResponseType, TimeSpan? timeout);

        void RegisterConsumer<T>(MessageLabel label, IConsumerOf<T> consumer, IMessageValidator validator)
            where T : class;

        void StartConsuming();

        void StopConsuming();

        bool Supports(MessageLabel label);
    }
}