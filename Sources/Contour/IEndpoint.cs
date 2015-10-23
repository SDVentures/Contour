namespace Contour
{
    /// <summary>
    ///   Описание конечной точки экземпляра шины.
    /// </summary>
    public interface IEndpoint
    {
        #region Public Properties

        /// <summary>
        ///   Адрес конечной точки.
        /// </summary>
        string Address { get; }

        #endregion
    }
}
