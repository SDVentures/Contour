// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LazyConsumerOf.cs" company="">
//   
// </copyright>
// <summary>
//   The lazy consumer of.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Threading.Tasks;

namespace Contour.Receiving.Consumers
{
    using System;

    /// <summary>
    /// The lazy consumer of.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public class LazyConsumerOf<T> : IConsumerOf<T>, IAsyncConsumerOf<T>
        where T : class
    {
        #region Fields

        /// <summary>
        /// The _handler.
        /// </summary>
        private readonly Lazy<IConsumer<T>> handler;

        private IConsumerOf<T> syncConsumer;

        private IAsyncConsumerOf<T> asyncConsumer;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="LazyConsumerOf{T}"/>.
        /// </summary>
        /// <param name="handlerResolver">
        /// The handler resolver.
        /// </param>
        public LazyConsumerOf(Func<object> handlerResolver)
        {
            this.handler = new Lazy<IConsumer<T>>(() => (IConsumer<T>)handlerResolver(), true);
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="LazyConsumerOf{T}"/>.
        /// </summary>
        /// <param name="handlerResolver">
        /// The handler resolver.
        /// </param>
        public LazyConsumerOf(Func<IConsumer<T>> handlerResolver)
        {
            this.handler = new Lazy<IConsumer<T>>(handlerResolver, true);
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
            if (!this.handler.IsValueCreated)
            {
                this.syncConsumer = this.handler.Value as IConsumerOf<T>;
            }

            if (this.syncConsumer == null)
            {
                throw new Exception($"This type: [{this.handler.Value.GetType()}] is not implement IConsumerOf<T>");
            }

            this.syncConsumer.Handle(context);
        }

        #endregion

        public Task HandleAsync(IConsumingContext<T> context)
        {
            if (!this.handler.IsValueCreated)
            {
                this.asyncConsumer = this.handler.Value as IAsyncConsumerOf<T>;
                if (this.asyncConsumer == null)
                {
                    this.syncConsumer = this.handler.Value as IConsumerOf<T>;
                }
            }

            if (this.asyncConsumer != null)
            {
                return this.asyncConsumer.HandleAsync(context);
            }

            if (this.syncConsumer != null)
            {
                this.syncConsumer.Handle(context);
                return Task.CompletedTask;
            }

            throw new Exception($"This type: [{this.handler.Value.GetType()}] is not implement IAsyncConsumerOf<T> or IConsumerOf<T>");
        }
    }
}
