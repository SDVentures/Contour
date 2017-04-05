// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IListeningSource.cs" company="">
//   
// </copyright>
// <summary>
//   The ListeningSource interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Contour.Receiving
{
    /// <summary>
    /// The ListeningSource interface.
    /// </summary>
    public interface IListeningSource
    {
        /// <summary>
        /// Gets the address.
        /// </summary>
        string Address { get; }
    }
}
