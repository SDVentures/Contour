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
    public class LazyAsyncConsumerOf<T> : IAsyncConsumerOf<T>
        where T : class
    {
        private readonly Lazy<IAsyncConsumerOf<T>> _handler;

        #region Constructors and Destructors

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="LazyConsumerOf{T}"/>.
        /// </summary>
        /// <param name="handlerResolver">
        /// The handler resolver.
        /// </param>
        public LazyAsyncConsumerOf(Func<object> handlerResolver) 
        {
            this._handler = new Lazy<IAsyncConsumerOf<T>>(() => (IAsyncConsumerOf<T>)handlerResolver(), true);
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="LazyConsumerOf{T}"/>.
        /// </summary>
        /// <param name="handlerResolver">
        /// The handler resolver.
        /// </param>
        public LazyAsyncConsumerOf(Func<IAsyncConsumerOf<T>> handlerResolver)
        {
            this._handler = new Lazy<IAsyncConsumerOf<T>>(handlerResolver, true);
        }

        #endregion

        #region Public Methods and Operators

        
        public async Task HandleAsync(IConsumingContext<T> context)
        {
            await this._handler.Value.HandleAsync(context).ConfigureAwait(false);
        }

        #endregion

        public void Handle(IConsumingContext<T> context)
        {
            throw new NotImplementedException();
        }
    }
}
