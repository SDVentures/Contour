using System.Threading.Tasks;

namespace Contour.Receiving.Consumers
{
    public interface IAsyncConsumerOf<T> : IConsumerOf<T>
        where T : class
    {
        #region Public Methods and Operators

        Task HandleAsync(IConsumingContext<T> context);

        #endregion
    }
}