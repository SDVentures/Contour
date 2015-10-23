namespace Contour.SampleConsumer
{
    /// <summary>
    /// The request.
    /// </summary>
    public class Request : OneWay
    {
        #region Constructors and Destructors

        /// <summary>
        /// »нициализирует новый экземпл€р класса <see cref="Request"/>.
        /// </summary>
        /// <param name="number">
        /// The number.
        /// </param>
        public Request(long number)
            : base(number)
        {
        }

        #endregion
    }
}
