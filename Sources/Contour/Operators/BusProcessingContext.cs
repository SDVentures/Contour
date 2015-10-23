namespace Contour.Operators
{
    using System;

    using Contour.Receiving;

    /// <summary>
    /// Контекст обработки сообщения.
    /// </summary>
    public class BusProcessingContext
    {
        [ThreadStatic]
        private static BusProcessingContext current;

        /// <summary>
        /// Текущий контекст.
        /// </summary>
        public static BusProcessingContext Current
        {
            get
            {
                return current;
            }

            set
            {
                current = value;
            }
        }

        /// <summary>
        /// Входящая доставка.
        /// </summary>
        public IDelivery Delivery { get; private set; }

        /// <summary>
        /// Создает объект контекста.
        /// </summary>
        /// <param name="delivery">Доставка.</param>
        public BusProcessingContext(IDelivery delivery)
        {
            this.Delivery = delivery;
        }
    }
}
