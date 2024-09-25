using System;

using Contour.Configuration;
using Contour.Helpers;

namespace Contour.Receiving
{
    /// <summary>
    /// Настройки получателя.
    /// </summary>
    public class ReceiverOptions : EndpointOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReceiverOptions"/> class. 
        /// </summary>
        public ReceiverOptions()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceiverOptions"/> class. 
        /// </summary>
        /// <param name="parent">
        /// Базовые настройки.
        /// </param>
        public ReceiverOptions(BusOptions parent)
            : base(parent)
        {
        }

        /// <summary>
        /// Устанавливает необходимость явного подтверждения успешной обработки.
        /// </summary>
        public Maybe<bool> AcceptIsRequired { protected get; set; }

        /// <summary>
        /// Построитель порта получения входящих сообщений.
        /// </summary>
        public Maybe<Func<ISubscriptionEndpointBuilder, ISubscriptionEndpoint>> EndpointBuilder { protected get; set; }

        /// <summary>
        /// Обработчик сообщений, чья доставка завершилась провалом.
        /// </summary>
        public Maybe<IFailedDeliveryStrategy> FailedDeliveryStrategy { protected get; set; }

        /// <summary>
        /// Количество одновременных обработчиков сообщений.
        /// </summary>
        public Maybe<uint> ParallelismLevel { protected get; set; }

        /// <summary>
        /// Длительность хранения сообщений в Fault очереди.
        /// </summary>
        public Maybe<TimeSpan> FaultQueueTtl { protected get; set; }

        /// <summary>
        /// Максимальное количество сообщений в Fault очереди.
        /// </summary>
        public Maybe<int> FaultQueueLimit { protected get; set; }

        /// <summary>
        /// Максимальное количество сообщений в потребляющей очереди.
        /// </summary>
        public Maybe<int> QueueLimit { protected get; set; }

        /// <summary>
        /// Максимальное количество байт, которые занимают сообщения в потребляющей очереди.
        /// </summary>
        public Maybe<int> QueueMaxLengthBytes { protected get; set; }


        /// <summary>
        /// Обработчик сообщений, для которых не найден потребитель.
        /// </summary>
        public Maybe<IUnhandledDeliveryStrategy> UnhandledDeliveryStrategy { protected get; set; }

        public bool Delayed { get; set; }

        public bool Direct { get; set; }

        public string DirectId { get; set; }

        /// <summary>
        /// Создает новый экземпляр настроек как копию существующего.
        /// </summary>
        /// <returns>
        /// Новый экземпляр настроек.
        /// </returns>
        public override BusOptions Derive()
        {
            return new ReceiverOptions(this);
        }

        /// <summary>
        /// Возвращает построитель порта получаемых сообщений.
        /// </summary>
        /// <returns>
        /// Построитель порта получаемых сообщений.
        /// </returns>
        public Maybe<Func<ISubscriptionEndpointBuilder, ISubscriptionEndpoint>> GetEndpointBuilder()
        {
            return this.Pick<ReceiverOptions, Func<ISubscriptionEndpointBuilder, ISubscriptionEndpoint>>((o) => o.EndpointBuilder);
        }

        /// <summary>
        /// Возвращает обработчик сообщений, для которых доставка завершилась провалом.
        /// </summary>
        /// <returns>
        /// Обработчик сообщений, для которых доставка завершилась провалом.
        /// </returns>
        public Maybe<IFailedDeliveryStrategy> GetFailedDeliveryStrategy()
        {
            return this.Pick<ReceiverOptions, IFailedDeliveryStrategy>((o) => o.FailedDeliveryStrategy);
        }

        /// <summary>
        /// Возвращает количество одновременных обработчиков сообщений.
        /// </summary>
        /// <returns>
        /// Количество одновременных обработчиков сообщений.
        /// </returns>
        public Maybe<uint> GetParallelismLevel()
        {
            return this.Pick<ReceiverOptions, uint>((o) => o.ParallelismLevel);
        }

        /// <summary>
        /// Возвращает обработчик сообщений, для которых не удалось найти потребителя.
        /// </summary>
        /// <returns>
        /// Обработчик сообщений, для которых не удалось найти потребителя.
        /// </returns>
        public Maybe<IUnhandledDeliveryStrategy> GetUnhandledDeliveryStrategy()
        {
            return this.Pick<ReceiverOptions, IUnhandledDeliveryStrategy>((o) => o.UnhandledDeliveryStrategy);
        }

        /// <summary>
        /// Возвращает признак необходимости явно подтверждать успешно обработанные сообщения.
        /// </summary>
        /// <returns>
        /// Если <c>true</c> - тогда необходимо подтверждать успешно обработанные сообщения, иначе - <c>false</c>.
        /// </returns>
        public Maybe<bool> IsAcceptRequired()
        {
            return this.Pick<ReceiverOptions, bool>((o) => o.AcceptIsRequired);
        }

        /// <summary>
        /// Возвращает длительность хранения сообщений в Fault очереди.
        /// </summary> 
        /// <returns>
        /// Возвращает длительность хранения сообщений в Fault очереди.
        /// </returns>
        public Maybe<TimeSpan> GetFaultQueueTtl()
        {
            return this.Pick<ReceiverOptions, TimeSpan>((o) => o.FaultQueueTtl);
        }

        /// <summary>
        /// Возвращает максимальное количество сообщений в Fault очереди.
        /// </summary>
        /// <returns>
        /// Возвращает максимальное количество сообщений в Fault очереди.
        /// </returns>
        public Maybe<int> GetFaultQueueLimit()
        {
            return this.Pick<ReceiverOptions, int>((o) => o.FaultQueueLimit);
        }

        /// <summary>
        /// Возвращает максимальное количество сообщений в потребляющей очереди.
        /// </summary>
        /// <returns>
        /// Возвращает максимальное количество сообщений в потребляющей очереди.
        /// </returns>
        public Maybe<int> GetQueueLimit()
        {
            return this.Pick<ReceiverOptions, int>((o) => o.QueueLimit);
        }

        /// <summary>
        /// Возвращает максимальное количество байт, которые занимають сообщения в потребляющей очереди.
        /// </summary>
        /// <returns>
        /// Возвращает максимальное количество байт, которые занимають сообщения в потребляющей очереди.
        /// </returns>
        public Maybe<int> GetQueueMaxLengthBytes()
        {
            return this.Pick<ReceiverOptions, int>((o) => o.QueueMaxLengthBytes);
        }
    }
}
