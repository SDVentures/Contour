// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QoSParams.cs" company="">
//   
// </copyright>
// <summary>
//   QoS настройки для RabbitMQ.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Contour.Transport.RabbitMQ
{
    /// <summary>
    ///   QoS настройки для RabbitMQ.
    /// </summary>
    public class QoSParams
    {
        #region Constructors and Destructors

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="QoSParams"/>. 
        /// The qo s params.
        /// </summary>
        /// <param name="prefetchCount">
        /// Количество сообщений получаемых из шины за одно обращение, т.е. размер порции данных.
        /// </param>
        /// <param name="prefetchSize">
        /// Количество сообщений, которые должен обработать получатель, прежде чем получит новую порцию
        ///   данных.
        /// </param>
        public QoSParams(ushort prefetchCount, uint prefetchSize)
        {
            this.PrefetchCount = prefetchCount;
            this.PrefetchSize = prefetchSize;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the prefetch count.
        /// </summary>
        public ushort PrefetchCount { get; private set; }

        /// <summary>
        /// Gets the prefetch size.
        /// </summary>
        public uint PrefetchSize { get; private set; }

        #endregion
    }
}
