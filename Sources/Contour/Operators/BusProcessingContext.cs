using System;
using Contour.Receiving;

namespace Contour.Operators
{
    /// <summary>
    /// Bus processing context used by Operators
    /// </summary>
    public class BusProcessingContext
    {
        [ThreadStatic]
        private static BusProcessingContext current;

        /// <summary>
        /// Initializes a new instance of the <see cref="BusProcessingContext"/> class. 
        /// </summary>
        /// <param name="delivery">Current delivery</param>
        [Obsolete("Must use constructor with IBusContext")]
        public BusProcessingContext(IDelivery delivery)
        {
            this.Delivery = delivery;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BusProcessingContext"/> class. 
        /// </summary>
        /// <param name="delivery">Current delivery</param>
        /// <param name="busContext">Bus context</param>
        public BusProcessingContext(IDelivery delivery, IBusContext busContext)
        {
            this.Delivery = delivery;
            this.BusContext = busContext;
        }

        /// <summary>
        /// Current context
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
        /// Incoming delivery
        /// </summary>
        public IDelivery Delivery { get; }

        /// <summary>
        /// Bus context
        /// </summary>
        public IBusContext BusContext { get; }
    }
}
