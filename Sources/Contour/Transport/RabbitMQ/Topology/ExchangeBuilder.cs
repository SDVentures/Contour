// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExchangeBuilder.cs" company="">
//   
// </copyright>
// <summary>
//   The exchange builder.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Contour.Transport.RabbitMQ.Topology
{
    using global::RabbitMQ.Client;

    /// <summary>
    /// The exchange builder.
    /// </summary>
    public class ExchangeBuilder
    {
        public const string DelayedExchangeType = "x-delayed-message";
        public const string DelayedExchangeSubtypeArgumentName = "x-delayed-type";
        
        #region Constructors and Destructors

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ExchangeBuilder"/>.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        internal ExchangeBuilder(string name)
        {
            this.Instance = new Exchange(name);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the auto delete.
        /// </summary>
        public ExchangeBuilder AutoDelete
        {
            get
            {
                this.Instance.AutoDelete = true;
                return this;
            }
        }

        /// <summary>
        /// Gets the direct.
        /// </summary>
        public ExchangeBuilder Direct
        {
            get
            {
                this.Instance.Type = ExchangeType.Direct;
                return this;
            }
        }

        /// <summary>
        /// Gets the durable.
        /// </summary>
        public ExchangeBuilder Durable
        {
            get
            {
                this.Instance.Durable = true;
                return this;
            }
        }

        /// <summary>
        /// Gets the fanout.
        /// </summary>
        public ExchangeBuilder Fanout
        {
            get
            {
                this.Instance.Type = ExchangeType.Fanout;
                return this;
            }
        }

        /// <summary>
        /// Gets the topic.
        /// </summary>
        public ExchangeBuilder Topic
        {
            get
            {
                this.Instance.Type = ExchangeType.Topic;
                return this;
            }
        }

        public ExchangeBuilder Headers
        {
            get
            {
                this.Instance.Type = ExchangeType.Headers;
                return this;
            }
        }

        public ExchangeBuilder DelayedDirect
        {
            get
            {
                this.Instance.Type = DelayedExchangeType;
                this.Instance.Arguments[DelayedExchangeSubtypeArgumentName] = ExchangeType.Direct;
                return this;
            }
        }

        public ExchangeBuilder DelayedFanout
        {
            get
            {
                this.Instance.Type = DelayedExchangeType;
                this.Instance.Arguments[DelayedExchangeSubtypeArgumentName] = ExchangeType.Fanout;
                return this;
            }
        }

        public ExchangeBuilder DelayedTopic
        {
            get
            {
                this.Instance.Type = DelayedExchangeType;
                this.Instance.Arguments[DelayedExchangeSubtypeArgumentName] = ExchangeType.Topic;
                return this;
            }
        }

        public ExchangeBuilder DelayedHeaders
        {
            get
            {
                this.Instance.Type = DelayedExchangeType;
                this.Instance.Arguments[DelayedExchangeSubtypeArgumentName] = ExchangeType.Headers;
                return this;
            }
        }
        
        #endregion

        #region Properties

        /// <summary>
        /// Gets the instance.
        /// </summary>
        internal Exchange Instance { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The of type.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// The <see cref="ExchangeBuilder"/>.
        /// </returns>
        public ExchangeBuilder OfType(string type)
        {
            this.Instance.Type = type;
            return this;
        }

        #endregion
    }
}
