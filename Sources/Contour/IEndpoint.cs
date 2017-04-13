namespace Contour
{
    /// <summary>
    ///   Описание конечной точки экземпляра шины.
    /// </summary>
    public interface IEndpoint
    {
        /// <summary>
        ///   Адрес конечной точки.
        /// </summary>
        string Address { get; }
    }
}
