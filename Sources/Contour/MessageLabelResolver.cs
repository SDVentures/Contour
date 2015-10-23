namespace Contour
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    using Contour.Configuration;

    /// <summary>
    /// The message label resolver.
    /// </summary>
    internal class MessageLabelResolver
    {
        #region Fields

        /// <summary>
        /// The _type labels.
        /// </summary>
        private readonly IDictionary<Type, MessageLabel> _typeLabels = new ConcurrentDictionary<Type, MessageLabel>();

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The resolve from.
        /// </summary>
        /// <param name="payloadType">
        /// The payload type.
        /// </param>
        /// <returns>
        /// The <see cref="MessageLabel"/>.
        /// </returns>
        public MessageLabel ResolveFrom(Type payloadType)
        {
            MessageLabel label;
            if (!this._typeLabels.TryGetValue(payloadType, out label))
            {
                label = GetLabelFrom(payloadType);
                this._typeLabels[payloadType] = label;
            }

            return label;
        }

        /// <summary>
        /// The resolve from.
        /// </summary>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="MessageLabel"/>.
        /// </returns>
        public MessageLabel ResolveFrom<T>() where T : class
        {
            return this.ResolveFrom(typeof(T));
        }

        #endregion

        #region Methods

        /// <summary>
        /// The get label from.
        /// </summary>
        /// <param name="payloadType">
        /// The payload type.
        /// </param>
        /// <returns>
        /// The <see cref="MessageLabel"/>.
        /// </returns>
        /// <exception cref="BusConfigurationException">
        /// </exception>
        private static MessageLabel GetLabelFrom(Type payloadType)
        {
            object attribute = payloadType.GetCustomAttributes(typeof(MessageAttribute), false).
                SingleOrDefault();
            if (attribute == null)
            {
                throw new BusConfigurationException("No MessageAttribute is defined for class [{0}].".FormatEx(payloadType.Name));
            }

            return ((MessageAttribute)attribute).Label.ToMessageLabel();
        }

        #endregion
    }
}
