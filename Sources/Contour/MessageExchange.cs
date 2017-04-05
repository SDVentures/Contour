namespace Contour
{
    using System;

    /// <summary>
    /// The message exchange.
    /// </summary>
    public class MessageExchange
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="MessageExchange"/>.
        /// </summary>
        /// <param name="out">
        /// The out.
        /// </param>
        /// <param name="expectedResponseType">
        /// The expected response type.
        /// </param>
        public MessageExchange(IMessage @out, Type expectedResponseType)
        {
            this.Out = @out;
            this.ExpectedResponseType = expectedResponseType;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="MessageExchange"/>.
        /// </summary>
        /// <param name="out">
        /// The out.
        /// </param>
        public MessageExchange(IMessage @out)
            : this(@out, null)
        {
        }
        /// <summary>
        /// Gets or sets the exception.
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Gets or sets the expected response type.
        /// </summary>
        public Type ExpectedResponseType { get; set; }

        /// <summary>
        /// Gets or sets the in.
        /// </summary>
        public IMessage In { get; set; }

        /// <summary>
        /// Gets a value indicating whether is complete request.
        /// </summary>
        public bool IsCompleteRequest
        {
            get
            {
                return this.IsRequest && (this.Out != null && this.In != null);
            }
        }

        /// <summary>
        /// Gets a value indicating whether is incomplete request.
        /// </summary>
        public bool IsIncompleteRequest
        {
            get
            {
                return this.IsRequest && (this.Out != null && this.In == null);
            }
        }

        /// <summary>
        /// Gets a value indicating whether is request.
        /// </summary>
        public bool IsRequest
        {
            get
            {
                return this.ExpectedResponseType != null;
            }
        }

        /// <summary>
        /// Gets or sets the out.
        /// </summary>
        public IMessage Out { get; set; }
        /// <summary>
        /// The throw if failed.
        /// </summary>
        /// <exception cref="Exception">
        /// </exception>
        public void ThrowIfFailed()
        {
            if (this.Exception != null)
            {
                throw this.Exception;
            }
        }
    }
}
