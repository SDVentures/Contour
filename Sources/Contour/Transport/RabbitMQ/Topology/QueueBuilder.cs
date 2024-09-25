using System;
using System.Collections.Generic;

namespace Contour.Transport.RabbitMQ.Topology
{
    /// <summary>
    /// Построитель очереди.
    /// </summary>
    public class QueueBuilder
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="QueueBuilder"/>.
        /// </summary>
        /// <param name="name">
        /// Имя очереди.
        /// </param>
        internal QueueBuilder(string name)
        {
            this.Instance = new Queue(name);
        }

        /// <summary>
        /// Очередь должна удаляться автоматически.
        /// </summary>
        public QueueBuilder AutoDelete
        {
            get
            {
                this.Instance.AutoDelete = true;
                return this;

                // TODO: на операции чтения (get) происходит изменение объекта. Надо исправить.
            }
        }

        /// <summary>
        /// Очередь должна быть надежной.
        /// </summary>
        public QueueBuilder Durable
        {
            get
            {
                this.Instance.Durable = true;
                return this;

                // TODO: на операции чтения (get) происходит изменение объекта. Надо исправить.
            }
        }

        /// <summary>
        /// Очередь должна быть эксклюзивной.
        /// </summary>
        public QueueBuilder Exclusive
        {
            get
            {
                this.Instance.Exclusive = true;
                return this;

                // TODO: на операции чтения (get) происходит изменение объекта. Надо исправить.
            }
        }

        /// <summary>
        /// Экземпляр очереди.
        /// </summary>
        internal Queue Instance { get; private set; }

        /// <summary>
        /// Добавляет в построитель настройки времени жизни сообщений в очереди.
        /// </summary>
        /// <param name="ttl">Время жизни сообщений в очереди.</param>
        /// <returns>Построитель очереди.</returns>
        public QueueBuilder WithTtl(TimeSpan ttl)
        {
            this.Instance.Ttl = ttl;
            return this;
        }

        /// <summary>
        /// Добавляет в построитель настройки максимального количества сообщений в очереди.
        /// </summary>
        /// <param name="queueLimit">Максимальное количество сообщений в очереди.</param>
        /// <returns>Построитель очереди.</returns>
        public QueueBuilder WithLimit(int queueLimit)
        {
            this.Instance.Limit = queueLimit;
            return this;
        }

        /// <summary>
        /// Добавляет в построитель настройки максимального количества байт, которое занимает очередь
        /// </summary>
        /// <param name="bytes">Максимальное количество сообщений в очереди.</param>
        /// <returns>Построитель очереди.</returns>
        public QueueBuilder WithMaxLengthBytes(int bytes)
        {
            this.Instance.MaxLengthBytes = bytes;
            return this;
        }
    }
}
