namespace Contour.Receiving.Consumers
{
    using System;

    /// <summary>
    /// The lazy consumer of.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public class LazyConsumerOf<T> : IConsumerOf<T>
        where T : class
    {
        /// <summary>
        /// The _handler.
        /// </summary>
        private readonly Lazy<IConsumerOf<T>> _handler;
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="LazyConsumerOf{T}"/>.
        /// </summary>
        /// <param name="handlerResolver">
        /// The handler resolver.
        /// </param>
        public LazyConsumerOf(Func<object> handlerResolver)
        {
            this._handler = new Lazy<IConsumerOf<T>>(() => (IConsumerOf<T>)handlerResolver(), true);
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="LazyConsumerOf{T}"/>.
        /// </summary>
        /// <param name="handlerResolver">
        /// The handler resolver.
        /// </param>
        public LazyConsumerOf(Func<IConsumerOf<T>> handlerResolver)
        {
            this._handler = new Lazy<IConsumerOf<T>>(handlerResolver, true);
        }
        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        public void Handle(IConsumingContext<T> context)
        {
            this._handler.Value.Handle(context);
        }
    }
}
