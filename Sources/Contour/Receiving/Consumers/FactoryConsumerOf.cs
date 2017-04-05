namespace Contour.Receiving.Consumers
{
    using System;

    /// <summary>
    /// The factory consumer of.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public class FactoryConsumerOf<T> : IConsumerOf<T>
        where T : class
    {
        /// <summary>
        /// The _handler resolver.
        /// </summary>
        private readonly Func<IConsumerOf<T>> _handlerResolver;
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
    }
}
