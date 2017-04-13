using System;
using System.Collections.Generic;
using System.Text;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Contour.Transport.RabbitMq.Internal
{
    /// <summary>
    /// The default rabbit message label handler.
    /// </summary>
    internal class DefaultRabbitMessageLabelHandler : IMessageLabelHandler
    {
        /// <summary>
        /// The inject.
        /// </summary>
        /// <param name="raw">
        /// The raw.
        /// </param>
        /// <param name="label">
        /// The label.
        /// </param>
        /// <exception cref="ArgumentException">
        /// </exception>
        public void Inject(object raw, MessageLabel label)
        {
            if (label.IsEmpty)
            {
                return;
            }

            var props = raw as IBasicProperties;
            if (props == null)
            {
                throw new ArgumentException(
                    "Expected [{0}] but got [{1}].".FormatEx(
                        typeof(IBasicProperties).Name, 
                        raw.GetType().
                            Name));
            }

            if (props.Headers == null)
            {
                props.Headers = new Dictionary<string, object>();
            }

            props.Headers[Headers.MessageLabel] = label.Name;
        }

        /// <summary>
        /// The resolve.
        /// </summary>
        /// <param name="raw">
        /// The raw.
        /// </param>
        /// <returns>
        /// The <see cref="MessageLabel"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// </exception>
        public MessageLabel Resolve(object raw)
        {
            var args = raw as BasicDeliverEventArgs;
            if (args == null)
            {
                throw new ArgumentException(
                    "Expected [{0}] but got [{1}].".FormatEx(
                        typeof(BasicDeliverEventArgs).Name, 
                        raw.GetType().
                            Name));
            }

            if (args.BasicProperties.Headers == null)
            {
                return MessageLabel.Empty;
            }

            object binaryLabel;
            if (!args.BasicProperties.Headers.TryGetValue(Headers.MessageLabel, out binaryLabel))
            {
                return MessageLabel.Empty;
            }

            return Encoding.UTF8.GetString((byte[])binaryLabel).
                ToMessageLabel();
        }
    }
}
