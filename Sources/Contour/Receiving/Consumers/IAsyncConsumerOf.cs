using System.Threading.Tasks;

namespace Contour.Receiving.Consumers
{   
    /// <summary>
    /// Асинхронный интерфейс, сделано наследование, что бы сохранить обратную совместимость, а так же дать возможность не править DI
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAsyncConsumerOf<T> : IConsumerOf<T>
        where T : class
    {
        #region Public Methods and Operators

        Task HandleAsync(IConsumingContext<T> context);

        #endregion
    }
}