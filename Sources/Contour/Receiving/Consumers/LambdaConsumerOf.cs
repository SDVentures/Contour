namespace Contour.Receiving.Consumers
{
    using System;

    /// <summary>
    /// The lambda consumer of.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public class LambdaConsumerOf<T> : IConsumerOf<T>
        where T : class
    {
        /// <summary>
        /// The _handler action.
        /// </summary>
        private readonly Action<IConsumingContext<T>> _handlerAction;
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="LambdaConsumerOf{T}"/>.
        /// </summary>
        /// <param name="handlerAction">
        /// The handler action.
        /// </param>
        public LambdaConsumerOf(Action<IConsumingContext<T>> handlerAction)
        {
            this._handlerAction = handlerAction;
        }
        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        public void Handle(IConsumingContext<T> context)
        {
            this._handlerAction(context);
        }
    }
}
