namespace Contour.SampleProducer
{
    /// <summary>
    /// The bad request.
    /// </summary>
    public class BadRequest : Message
    {
        #region Constructors and Destructors

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="BadRequest"/>.
        /// </summary>
        /// <param name="number">
        /// The number.
        /// </param>
        public BadRequest(long number)
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
