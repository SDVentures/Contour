using System.Collections.Generic;
using System.Threading.Tasks;

using Common.Logging;

using Contour.Filters;
using Contour.Sending;

namespace Contour.Transport.RabbitMQ.Internal
{
    /// <summary>
    /// Отправитель сообщений с помощью брокера <c>RabbitMQ</c>.
    /// </summary>
    internal class RabbitSender : AbstractSender
    {
        /// <summary>
        /// Журнал выполнения отправителя.
        /// </summary>
        private static readonly ILog Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Реестр поставщиков сообщений.
        /// </summary>
        private readonly ProducerRegistry producerRegistry;

        /// <summary>
        /// Поставщик сообщений.
        /// </summary>
        private Producer producer;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="RabbitSender"/>.
        /// </summary>
        /// <param name="endpoint">Конечная точка, для которой создается отправитель.</param>
        /// <param name="configuration">Конфигурация отправителя сообщений.</param>
        /// <param name="producerRegistry">Реестр поставщиков сообщений.</param>
        /// <param name="filters">Фильтры сообщений.</param>
        public RabbitSender(IEndpoint endpoint, ISenderConfiguration configuration, ProducerRegistry producerRegistry, IEnumerable<IMessageExchangeFilter> filters)
            : base(endpoint, configuration, filters)
        {
            this.producerRegistry = producerRegistry;
        }

        /// <summary>
        /// Если <c>true</c> - запущен, иначе <c>false</c>.
        /// </summary>
        public bool IsStarted { get; private set; }

        /// <summary>
        /// Если <c>true</c> - отправитель работает без сбоев, иначе <c>false</c>.
        /// </summary>
        public override bool IsHealthy
        {
            get
            {
                return !producer.HasFailed;
            }
        }

        /// <summary>
        /// Освобождает занятые ресурсы. И останавливает отправителя.
        /// </summary>
        public override void Dispose()
        {
            Logger.Trace(m => m("Disposing sender of [{0}].", this.Configuration.Label));
            this.Stop();
        }

        /// <summary>
        /// Запускает отправителя.
        /// </summary>
        public override void Start()
        {
            Logger.Trace(m => m("Starting sender of [{0}].", this.Configuration.Label));

            if (this.IsStarted)
            {
                return;
            }

            this.IsStarted = true;

            this.EnsureProducerIsReady();
        }

        /// <summary>
        /// Останавливает отправителя.
        /// </summary>
        public override void Stop()
        {
            Logger.Trace(m => m("Stopping sender of [{0}].", this.Configuration.Label));
            this.IsStarted = false;

            this.UnbindProducer();
        }

        /// <summary>
        /// Выполняет отправку сообщения.
        /// </summary>
        /// <param name="exchange">Информация об отправке.</param>
        /// <returns>Задача ожидания отправки сообщения.</returns>
        protected override Task<MessageExchange> InternalSend(MessageExchange exchange)
        {
            this.EnsureProducerIsReady();

            if (exchange.IsRequest)
            {
                return this.producer.Request(exchange.Out, exchange.ExpectedResponseType)
                    .ContinueWith(
                        t =>
                            {
                                if (t.IsFaulted)
                                {
                                    exchange.Exception = t.Exception;
                                }
                                else
                                {
                                    exchange.In = t.Result;
                                }

                                return exchange;
                            });
            }

            return this.producer.Publish(exchange.Out)
                .ContinueWith(
                    t =>
                        {
                            if (t.IsFaulted)
                            {
                                exchange.Exception = t.Exception;
                            }

                            return exchange;
                        });
        }

        /// <summary>
        /// Гарантирует, что поставщик сообщений запущен.
        /// </summary>
        private void EnsureProducerIsReady()
        {
            if (this.producer == null)
            {
                Logger.Trace(m => m("Resolving producer for sender of [{0}].", this.Configuration.Label));
                this.producer = this.producerRegistry.ResolveFor(this.Configuration);
                this.producer.Start();
            }
        }

        /// <summary>
        /// Отписывается от поставщика сообщений.
        /// </summary>
        private void UnbindProducer()
        {
            Logger.Trace(m => m("Unbinding producer from sender of [{0}].", this.Configuration.Label));
            this.producer = null;
        }
    }
}
