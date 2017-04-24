namespace Contour.SampleProducer
{
    /// <summary>
    /// The one way.
    /// </summary>
    public class OneWay : Message
    {
        #region Constructors and Destructors

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="OneWay"/>.
        /// </summary>
        /// <param name="number">
        /// The number.
        /// </param>
        public OneWay(long number)
        {
            this.Number = number;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="OneWay"/>.
        /// </summary>
        protected OneWay()
        {
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the number.
        /// </summary>
        public long Number { get; private set; }

        #endregion
    }
}
