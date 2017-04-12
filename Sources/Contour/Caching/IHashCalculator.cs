namespace Contour.Caching
{
    /// <summary>
    /// Calculates a hash of the message payload.
    /// </summary>
    public interface IHashCalculator
    {
        /// <summary>
        /// Calculate a hash of the message payload.
        /// </summary>
        /// <param name="message">The message to be calculated.</param>
        /// <returns>Hash value.</returns>
        string CalculateHash(IMessage message);
    }
}