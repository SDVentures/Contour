using System.Threading.Tasks;

namespace Contour.Receiving.Consumers
{
    using System;

    /// <summary>
    /// The factory consumer of.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public class FactoryConsumerOf<T> : IAsyncConsumerOf<T>
        where T : class
    {
        #region Fields

        /// <summary>
        /// The _handler resolver.
        /// </summary>
        private readonly Func<IConsumerOf<T>> _handlerResolver;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="FactoryConsumerOf{T}"/>.
        /// </summary>
        /// <param name="handlerResolver">
        /// The handler resolver.
        /// </param>
        public FactoryConsumerOf(Func<IConsumerOf<T>> handlerResolver)
        {
            this._handlerResolver = handlerResolver;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="FactoryConsumerOf{T}"/>.
        /// </summary>
        /// <param name="handlerResolver">
        /// The handler resolver.
        /// </param>
        public FactoryConsumerOf(Func<object> handlerResolver)
        {
            this._handlerResolver = () => (IConsumerOf<T>)handlerResolver();
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        public void Handle(IConsumingContext<T> context)
        {
            this._handlerResolver().
                Handle(context);
        }

        public async Task HandleAsync(IConsumingContext<T> context)
        {
            if (this._handlerResolver() is IAsyncConsumerOf<T> of)
            {
                await of.HandleAsync(context);
            }
            else
            {
                throw new Exception();
            }
        }

        #endregion
    }
}
