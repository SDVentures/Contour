using System.Collections.Generic;

using Contour.Helpers;
using Contour.Receiving;
using Contour.Receiving.Consumers;

namespace Contour.Operators
{
    /// <summary>
    /// Потребитель сообщения, который применяет оператор для обработки сообщения.
    /// </summary>
    /// <typeparam name="T">Тип сообщения.</typeparam>
    public class OperatorConsumerOf<T> : IConsumerOf<T>
        where T : class
    {
        private readonly IMessageOperator @operator;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="OperatorConsumerOf{T}"/>.
        /// </summary>
        /// <param name="operator">
        /// Оператор применяемый для обработки сообщения.
        /// </param>
        public OperatorConsumerOf(IMessageOperator @operator)
        {
            this.@operator = @operator;
        }

        /// <summary>
        /// Обрабатывает входящее сообщение.
        /// </summary>
        /// <param name="context">Контекст обработки сообщения.</param>
        public void Handle(IConsumingContext<T> context)
        {
            BusProcessingContext.Current = new BusProcessingContext(((IDeliveryContext)context).Delivery);

            this.@operator
                .Apply(context.Message)
                .ForEach(
                    m =>
                        {
                            var headers = new Dictionary<string, object>(m.Headers);
                            Headers.ApplyBreadcrumbs(headers, context.Bus.Endpoint.Address);
                            Headers.ApplyOriginalMessageId(headers);
                            context.Bus.Emit(m.Label, m.Payload, headers);
                        });

            context.Accept();
        }
    }
}
