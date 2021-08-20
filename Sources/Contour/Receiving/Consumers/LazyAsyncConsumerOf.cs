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
    public class LazyAsyncConsumerOf<T> : LazyConsumerOf<T>, IAsyncConsumerOf<T>
        where T : class
    {
        protected new readonly Lazy<IAsyncConsumerOf<T>> _handler;

        #region Constructors and Destructors

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="LazyConsumerOf{T}"/>.
        /// </summary>
        /// <param name="handlerResolver">
        /// The handler resolver.
        /// </param>
        public LazyAsyncConsumerOf(Func<object> handlerResolver) : base(handlerResolver)
        {
            this._handler = new Lazy<IAsyncConsumerOf<T>>(() => (IAsyncConsumerOf<T>)handlerResolver(), true);
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="LazyConsumerOf{T}"/>.
        /// </summary>
        /// <param name="handlerResolver">
        /// The handler resolver.
        /// </param>
        public LazyAsyncConsumerOf(Func<IAsyncConsumerOf<T>> handlerResolver) : base(handlerResolver)
        {
            this._handler = new Lazy<IAsyncConsumerOf<T>>(handlerResolver, true);
        }

        #endregion

        #region Public Methods and Operators

        
        public async Task HandleAsync(IConsumingContext<T> context)
        {
            if (this._handler.Value is IAsyncConsumerOf<T> of)
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
