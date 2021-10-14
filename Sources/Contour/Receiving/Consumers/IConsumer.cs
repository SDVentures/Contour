// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IConsumer.cs" company="">
//   
// </copyright>
// <summary>
//   The Consumer interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Contour.Receiving.Consumers
{
    /// <summary>
    /// The Generic Consumer interface. Нужен что бы не потерять тип при резолве зависимостей.
    /// </summary>
    public interface IConsumer<T> : IConsumer
    {
    }

    /// <summary>
    /// The Consumer interface.
    /// </summary>
    public interface IConsumer
    {
    }
}
