namespace Contour.SampleConsumer
{
    /// <summary>
    /// The one way.
    /// </summary>
    public class OneWay
    {
        #region Constructors and Destructors

        /// <summary>
        /// »нициализирует новый экземпл€р класса <see cref="OneWay"/>.
        /// </summary>
        /// <param name="number">
        /// The number.
        /// </param>
        public OneWay(long number)
        {
            this.Number = number;
        }

        /// <summary>
        /// »нициализирует новый экземпл€р класса <see cref="OneWay"/>.
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
