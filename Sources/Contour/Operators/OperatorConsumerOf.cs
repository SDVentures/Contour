using System.Collections.Generic;
using Contour.Helpers;
using Contour.Receiving;
using Contour.Receiving.Consumers;

namespace Contour.Operators
{
    /// <summary>
    /// Message consumer that uses <see cref="IMessageOperator"/> to process received messages.
    /// All messages received from the call to Apply method of <see cref="IMessageOperator"/> are sent through <see cref="IBusContext"/>
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    public class OperatorConsumerOf<T> : IConsumerOf<T>
        where T : class
    {
        private readonly IMessageOperator @operator;

        /// <summary>
        /// Initializes a new instance of the <see cref="OperatorConsumerOf{T}"/> class. 
        /// </summary>
        /// <param name="operator">Operator applied for message processing</param>
        public OperatorConsumerOf(IMessageOperator @operator)
        {
            this.@operator = @operator;
        }

        /// <inheritdoc />
        public void Handle(IConsumingContext<T> context)
        {
            BusProcessingContext.Current = new BusProcessingContext(((IDeliveryContext)context).Delivery, context.Bus);

            this.@operator
                .Apply(context.Message)
                .ForEach(
                    m =>
                        {
                            var headers = new Dictionary<string, object>(m.Headers);
                            Headers.ApplyBreadcrumbs(headers, context.Bus.Endpoint.Address);
                            Headers.ApplyOriginalMessageId(headers);
                            Headers.ApplyMessageId(headers);
                            context.Bus.Emit(m.Label, m.Payload, headers);
                        });

            context.Accept();
        }
    }
}
