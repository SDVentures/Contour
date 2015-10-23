namespace Contour.SampleConsumer
{
    /// <summary>
    /// The response.
    /// </summary>
    public class Response : OneWay
    {
        #region Constructors and Destructors

        /// <summary>
        /// »нициализирует новый экземпл€р класса <see cref="Response"/>.
        /// </summary>
        /// <param name="number">
        /// The number.
        /// </param>
        public Response(long number)
            : base(number)
        {
        }

        #endregion
    }
}
