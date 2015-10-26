namespace Contour.Helpers.Timing
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Timers;

    using Timer = System.Timers.Timer;

    /// <summary>
    /// Планировщик, который вызывает выполнение задачи по истечении заданного времени ожидания.
    /// Проверка необходимости выполнить задачи происходит с определенным интервалом. Фактически, величина этого интервала задает точность, 
    /// с которой будут вызываться задачи. 
    /// Задача автоматически удаляется из планировщика при вызове на выполнение.
    /// При регистрации задачи в планировщике возвращается квитанция, на основе которой, можно удалить задачу из планировщика.
    /// </summary>
    internal class RoughTicketTimer : ITicketTimer
    {
        /// <summary>
        /// Словарь квитанций и задач на выполнение.
        /// </summary>
        private readonly IDictionary<long, Job> jobs = new ConcurrentDictionary<long, Job>();

        /// <summary>
        /// Таймер, который вызывает проверку на выполнение задач через заданные интервалы времени.
        /// </summary>
        private readonly Timer timer;

        /// <summary>
        /// Номер последней выданной квитанции задачи.
        /// </summary>
        private long lastTicketNo;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="RoughTicketTimer"/>.
        /// </summary>
        /// <param name="checkInterval">
        /// Интервал времени, с которым будет проверяться необходимость выполнить задачи.
        /// </param>
        public RoughTicketTimer(TimeSpan checkInterval)
        {
            this.timer = new Timer(checkInterval.TotalMilliseconds);
            this.timer.Elapsed += this.TimerOnElapsed;
            this.timer.Start();
        }

        /// <summary>
        /// Уничтожает экземпляр класса <see cref="RoughTicketTimer"/>. 
        /// </summary>
        ~RoughTicketTimer()
        {
            this.Dispose();
        }

        /// <summary>
        /// Возвращает количество задач, которые ожидают своего выполнения.
        /// </summary>
        public int JobCount
        {
            get
            {
                return this.jobs.Count;
            }
        }

        /// <summary>
        /// Регистрирует задачу в планировщике.
        /// </summary>
        /// <param name="span">
        /// Интервал, через который должна быть вызвана задача.
        /// </param>
        /// <param name="callback">
        /// Действие вызываемое при выполнении задачи.
        /// </param>
        /// <returns>
        /// Квитанция зарегистрированной задачи. Квитанция может быть использована для удаления задачи из планировщика.
        /// </returns>
        public long Acquire(TimeSpan span, Action callback)
        {
            long ticket = Interlocked.Increment(ref this.lastTicketNo);
            this.jobs.Add(ticket, new Job(DateTime.Now.Add(span), callback));
            return ticket;
        }

        /// <summary>
        /// Отменяет выполнение задачи в планировщике.
        /// Задача будет удалена.
        /// </summary>
        /// <param name="ticket">
        /// Квитанция задачи, которую нужно отменить.
        /// </param>
        public void Cancel(long ticket)
        {
            this.jobs.Remove(ticket);
        }

        /// <summary>
        /// Освобождает занятые ресурсы.
        /// </summary>
        public void Dispose()
        {
            if (this.timer != null)
            {
                this.timer.Stop();
                this.timer.Dispose();
            }
        }

        /// <summary>
        /// Обработчик события об истекшем времени ожидания.
        /// Проверяет необходимость выполнить задачи.
        /// Если есть задачи, которые нужно выполнить, тогда выполняет их и удаляет из планировщика.
        /// </summary>
        /// <param name="sender">
        /// Инициатор события.
        /// </param>
        /// <param name="elapsedEventArgs">
        /// Информация о событии.
        /// </param>
        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            DateTime now = DateTime.Now;

            foreach (var ticket in this.jobs)
            {
                if (ticket.Value.Expires < now)
                {
                    this.jobs.Remove(ticket);
                    try
                    {
                        ticket.Value.Callback();
                    }
                    // ReSharper disable once EmptyGeneralCatchClause
                    catch
                    {
                        // TODO: something better than eating exceptions
                    }
                }
            }
        }

        /// <summary>
        /// Выполняемая задача.
        /// </summary>
        public class Job
        {
            // TODO: скрыть класс от публичного использования.

            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="Job"/>.
            /// </summary>
            /// <param name="expires">
            /// Время, когда необходимо выполнить задачу.
            /// </param>
            /// <param name="callback">
            /// Действие вызываемое при выполнении задачи.
            /// </param>
            public Job(DateTime expires, Action callback)
            {
                this.Expires = expires;
                this.Callback = callback;
            }

            /// <summary>
            /// Действие вызываемое при выполнении задачи.
            /// </summary>
            public Action Callback { get; private set; }

            /// <summary>
            /// Время, когда необходимо выполнить задачу.
            /// </summary>
            public DateTime Expires { get; private set; }
        }
    }
}
