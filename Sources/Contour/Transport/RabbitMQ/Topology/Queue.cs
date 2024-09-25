using System;

using Contour.Receiving;
using Contour.Topology;

namespace Contour.Transport.RabbitMQ.Topology
{
    /// <summary>
    /// Описание очереди <c>RabbitMq</c>.
    /// </summary>
    public class Queue : ITopologyEntity, IListeningSource
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Queue"/>.
        /// </summary>
        /// <param name="name">
        /// Имя очереди.
        /// </param>
        internal Queue(string name)
        {
            this.Name = name;
            this.Durable = false;
            this.Exclusive = false;
            this.AutoDelete = false;
        }

        /// <summary>
        /// Адрес очереди.
        /// </summary>
        public string Address => this.Name;

        /// <summary>
        /// Признак удаляемой очереди.
        /// </summary>
        public bool AutoDelete { get; internal set; }

        /// <summary>
        /// Признак надежности очереди.
        /// </summary>
        public bool Durable { get; internal set; }

        /// <summary>
        /// Признак эксклюзивности очереди.
        /// </summary>
        public bool Exclusive { get; internal set; }

        /// <summary>
        /// Имя очереди.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Время жизни сообщений в очереди.
        /// </summary>
        public TimeSpan? Ttl { get; internal set; }

        /// <summary>
        /// Максимальное количество сообщений в очереди.
        /// </summary>
        public int? Limit { get; internal set; }

        /// <summary>
        /// Максимальное количество байт, которое занимают сообщения в очереди.
        /// </summary>
        public int? MaxLengthBytes { get; internal set; }

        /// <summary>
        /// Создает экземпляр построителя очереди с именем <paramref name="name"/>.
        /// </summary>
        /// <param name="name">
        /// Имя создаваемой очереди.
        /// </param>
        /// <returns>
        /// Построитель очереди <see cref="QueueBuilder"/>.
        /// </returns>
        public static QueueBuilder Named(string name)
        {
            return new QueueBuilder(name);

            // TODO: странная зависимость создаваемого объекта от создателя.
        }

        /// <summary>
        /// Выполняет сравнение двух очередей.
        /// </summary>
        /// <param name="obj">
        /// Сравниваемая очередь.
        /// </param>
        /// <returns>
        /// <c>true</c> - если объекты равны, иначе <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return this.Equals((Queue)obj);
        }

        /// <summary>
        /// Рассчитывает код хеширования для очереди.
        /// </summary>
        /// <returns>
        /// Код хеширования значение очереди.
        /// </returns>
        public override int GetHashCode()
        {
            return this.Name != null ? this.Name.GetHashCode() : 0;
        }

        /// <summary>
        /// Конвертирует очередь в строку.
        /// </summary>
        /// <returns>
        /// Строковое представление очереди.
        /// </returns>
        public override string ToString()
        {
            return $"{this.Name}";
        }

        /// <summary>
        /// Выполняет сравнение двух очередей.
        /// </summary>
        /// <param name="other">
        /// Сравниваемая очередь.
        /// </param>
        /// <returns>
        /// <c>true</c> - если объекты равны, иначе <c>false</c>.
        /// </returns>
        protected bool Equals(Queue other)
        {
            return string.Equals(this.Name, other.Name);
        }
    }
}
