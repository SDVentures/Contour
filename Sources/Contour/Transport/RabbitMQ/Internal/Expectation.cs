using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Contour.Receiving;

namespace Contour.Transport.RabbitMQ.Internal
{
    /// <summary>
    /// Ожидание ответа на запрос.
    /// </summary>
    internal class Expectation
    {
        /// <summary>
        /// Источник сигнальных объектов об аварийном завершении задачи.
        /// </summary>
        private readonly TaskCompletionSource<IMessage> completionSource;

        /// <summary>
        /// Построитель ответа.
        /// </summary>
        private readonly Func<IDelivery, IMessage> responseBuilderFunc;

        /// <summary>
        /// Секундомер для замера длительности ожидания ответа.
        /// </summary>
        private readonly Stopwatch completionStopwatch;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Expectation"/>.
        /// </summary>
        /// <param name="responseBuilderFunc">
        /// Построитель ответа.
        /// </param>
        /// <param name="timeoutTicket">
        /// Квиток об учете времени ожидания ответа.
        /// </param>
        public Expectation(Func<IDelivery, IMessage> responseBuilderFunc, long? timeoutTicket)
        {
            this.responseBuilderFunc = responseBuilderFunc;
            this.TimeoutTicket = timeoutTicket;

            this.completionSource = new TaskCompletionSource<IMessage>();
            this.completionStopwatch = Stopwatch.StartNew();
        }

        /// <summary>
        /// Задача завершения ожидания.
        /// </summary>
        public Task<IMessage> Task
        {
            get
            {
                return this.completionSource.Task;
            }
        }

        /// <summary>
        /// Квиток об учете времени ожидания ответа.
        /// </summary>
        public long? TimeoutTicket { get; private set; }

        /// <summary>
        /// Отменяет ожидание ответа.
        /// </summary>
        public void Cancel()
        {
            this.completionSource.TrySetException(new OperationCanceledException());
        }

        /// <summary>
        /// Выполняет обработку ответа на запрос.
        /// </summary>
        /// <param name="delivery">
        /// Входящее сообщение - ответ на запрос.
        /// </param>
        public void Complete(RabbitDelivery delivery)
        {
            try
            {
                this.completionStopwatch.Stop();
                IMessage response = this.responseBuilderFunc(delivery);
                this.completionSource.TrySetResult(response);
            }
            catch (Exception ex)
            {
                this.completionSource.TrySetException(ex);
                throw;
            }
        }

        /// <summary>
        /// Устанавливает, что при ожидании вышло время, за которое должен был быть получен ответ.ё
        /// </summary>
        public void Timeout()
        {
            this.completionSource.TrySetException(new TimeoutException());
        }
    }
}