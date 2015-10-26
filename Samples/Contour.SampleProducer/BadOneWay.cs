namespace Contour.SampleProducer
{
    /// <summary>
    /// The bad one way.
    /// </summary>
    public class BadOneWay : Message
    {
        #region Constructors and Destructors

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="BadOneWay"/>.
        /// </summary>
        /// <param name="number">
        /// The number.
        /// </param>
        public BadOneWay(long number)
        {
            this.Number = "The winner is " + number;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the number.
        /// </summary>
        public string Number { get; private set; }

        #endregion
    }
}
