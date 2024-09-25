using System;
using System.Collections.Generic;

using Contour.Configuration;
using Contour.Helpers;
using Contour.Sending;
using Contour.Transport.RabbitMQ.Internal;
using Contour.Transport.RabbitMQ.Topology;

namespace Contour.Transport.RabbitMQ
{
    /// <summary>
    /// Методы расширения конфигуратора.
    /// </summary>
    public static class BusConfigurationEx
    {
        /// <summary>
        /// Количество дней, которое хранятся сообщения <c>Fault</c> очередей.
        /// </summary>
        private static readonly int FaultMessageTtlDays = 21;

        /// <summary>
        /// Устанавливает параметры <c>Quality of service (QoS)</c> по умолчанию для всех получателей.
        /// </summary>
        /// <param name="busConfigurator">Конфигуратор шины сообщений.</param>
        /// <param name="prefetchCount">Количество сообщений получаемых из шины за одно обращение, т.е. размер порции данных.</param>
        /// <param name="prefetchSize">Количество сообщений, которые должен обработать получатель, прежде чем получит новую порцию данных.</param>
        /// <returns>Конфигурация экземпляра шины сообщений.</returns>
        public static IBusConfigurator SetDefaultQoS(this IBusConfigurator busConfigurator, ushort prefetchCount, uint prefetchSize = 0) 
        {
            // TODO: beautify
            ((RabbitReceiverOptions)((BusConfiguration)busConfigurator).ReceiverDefaults).QoS = new QoSParams(prefetchCount, prefetchSize);

            return busConfigurator;
        }

        /// <summary>
        /// Включает использование брокера <c>RabbitMQ</c>.
        /// </summary>
        /// <param name="busConfigurator">Конфигуратор шины сообщений.</param>
        /// <returns>Конфигуратор шины сообщений с включенной поддержкой брокера.</returns>
        public static IBusConfigurator UseRabbitMq(this IBusConfigurator busConfigurator)
        {
            var c = (BusConfiguration)busConfigurator;
            if (c.BusFactoryFunc != null)
            {
                return busConfigurator;
            }

            c.BuildBusUsing(bc => new RabbitBus(c));

            var blockedHeaders = new List<string>
                                     {
                                         Headers.Expires,
                                         Headers.MessageLabel,
                                         Headers.Persist,
                                         Headers.QueueMessageTtl,
                                         Headers.ReplyRoute,
                                         Headers.Timeout,
                                         Headers.Ttl,
                                         Headers.QueueMaxLength,
                                         Headers.QueueMaxLengthBytes,
                                         Headers.Delay,
                                         Headers.DirectId,
                                         Headers.MatchHeaders
                                     };
            var messageHeaderStorage = new MessageHeaderStorage(blockedHeaders);
            c.UseIncomingMessageHeaderStorage(messageHeaderStorage);

            if (Headers.GlobalStorage == null)
            {
                // hack to give access to headers if needed
                Headers.GlobalStorage = messageHeaderStorage;
            }

            c.SenderDefaults = new RabbitSenderOptions(c.EndpointOptions)
            {
                ConfirmationIsRequired = false,
                Persistently = false,
                RequestTimeout = TimeSpan.FromSeconds(30),
                Ttl = default(TimeSpan?),
                RouteResolverBuilder = RabbitBusDefaults.RouteResolverBuilder,
                ReuseConnection = true,
                ProducerSelectorBuilder = RabbitBusDefaults.ProducerSelectorBuilder,
                FailoverAttempts = 7,
                MaxRetryDelay = 30,
                InactivityResetDelay = 120
            };

            c.ReceiverDefaults = new RabbitReceiverOptions(c.EndpointOptions)
            {
                AcceptIsRequired = false,
                ParallelismLevel = 1,
                EndpointBuilder = RabbitBusDefaults.SubscriptionEndpointBuilder,
                QoS = new QoSParams(50, 0),
                ReuseConnection = true
            };

            c.UseMessageLabelHandler(new DefaultRabbitMessageLabelHandler());
           
            // TODO: extract, combine routing and handler definition
            Func <IRouteResolverBuilder, IRouteResolver> faultRouteResolverBuilder = b =>
                {
                    TimeSpan messageTtl = c.ReceiverDefaults.GetFaultQueueTtl().HasValue
                                              ? c.ReceiverDefaults.GetFaultQueueTtl().Value
                                              : TimeSpan.FromDays(FaultMessageTtlDays);

                    string name = b.Endpoint.Address + ".Fault";
                    Exchange e = b.Topology.Declare(Exchange.Named(name).Durable.Topic);
                    QueueBuilder builder = Queue.Named(name).Durable.WithTtl(messageTtl);

                    if (c.ReceiverDefaults.GetFaultQueueLimit().HasValue)
                    {
                        builder = builder.WithLimit(c.ReceiverDefaults.GetFaultQueueLimit().Value);
                    }
                    Queue q = b.Topology.Declare(builder);
                    b.Topology.Bind(e, q);
                    return e;
                };

            c.Route(SystemQueues.Unhandled).Persistently().ConfiguredWith(faultRouteResolverBuilder).ReuseConnection();
            c.Route(SystemQueues.Fault).Persistently().ConfiguredWith(faultRouteResolverBuilder).ReuseConnection();

            c.OnUnhandled(
                d =>
                    {
                        d.Forward(SystemQueues.Unhandled, d.BuildFaultMessage());
                        d.Accept();
                    });
            c.OnFailed(
                d =>
                    {
                        d.Forward(SystemQueues.Fault, d.BuildFaultMessage());
                        d.Accept();
                    });

            return busConfigurator;
        }
    }
}
