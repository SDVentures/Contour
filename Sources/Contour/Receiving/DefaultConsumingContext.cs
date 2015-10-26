using System.Collections.Generic;

using Contour.Caching;

namespace Contour.Receiving
{
    /// <summary>
    /// Контекст обработки полученного сообщения.
    /// </summary>
    /// <typeparam name="T">Тип полученного сообщения.</typeparam>
    internal class DefaultConsumingContext<T> : IConsumingContext<T>, IDeliveryContext
        where T : class
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="DefaultConsumingContext{T}"/>.
        /// </summary>
        /// <param name="message">Входящее сообщение.</param>
        /// <param name="delivery">Полученное сообщение.</param>
        public DefaultConsumingContext(Message<T> message, IDelivery delivery)
        {
            this.Message = message;
            this.Delivery = delivery;
            this.Bus = delivery.Channel.Bus;
        }

        /// <summary>
        /// Конечная точка, в которой зарегистрирован текущий обработчик сообщения.
        /// </summary>
        public IBusContext Bus { get; private set; }

        /// <summary>
        /// Возвращает доставленное сообщение.
        /// </summary>
        public IDelivery Delivery { get; private set; }

        /// <summary>
        /// Полученное сообщение.
        /// </summary>
        public Message<T> Message { get; private set; }

        /// <summary>
        /// Верно, если можно ответить на это сообщение.
        /// </summary>
        public bool CanReply
        {
            get
            {
                return this.Delivery.CanReply;
            }
        }

        /// <summary>
        /// Помечает сообщение как обработанное.
        /// </summary>
        public void Accept()
        {
            this.Delivery.Accept();
        }

        /// <summary>
        /// Пересылает сообщение, устанавливая указанную метку.
        /// </summary>
        /// <param name="label">Новая метка, с которой пересылается сообщение.</param>
        public void Forward(MessageLabel label)
        {
            this.Forward(label, this.Message.Payload);
        }

        /// <summary>
        /// Пересылает сообщение, устанавливая указанную метку.
        /// </summary>
        /// <param name="label">Новая метка, с которой пересылается сообщение.</param>
        public void Forward(string label)
        {
            this.Forward(label, this.Message.Payload);
        }

        /// <summary>
        /// Пересылает сообщение, устанавливая указанную метку.
        /// </summary>
        /// <param name="label">Новая метка, с которой пересылается сообщение.</param>
        /// <param name="payload">Новое содержимое сообщения.</param>
        /// <typeparam name="TOut">Тип сообщения.</typeparam>
        public void Forward<TOut>(MessageLabel label, TOut payload = default(TOut)) where TOut : class
        {
            this.Delivery.Forward(label, payload);
        }

        /// <summary>
        /// Пересылает сообщение, устанавливая указанную метку.
        /// </summary>
        /// <param name="label">Новая метка, с которой пересылается сообщение.</param>
        /// <param name="payload">Новое содержимое сообщения.</param>
        /// <typeparam name="TOut">Тип сообщения.</typeparam>
        public void Forward<TOut>(string label, TOut payload = default(TOut)) where TOut : class
        {
            this.Forward(label.ToMessageLabel(), payload);
        }

        /// <summary>
        /// Помечает сообщение как необработанное.
        /// </summary>
        /// <param name="requeue">
        /// Сообщение требуется вернуть во входящую очередь для повторной обработки.
        /// </param>
        public void Reject(bool requeue)
        {
            this.Delivery.Reject(requeue);
        }

        /// <summary>
        /// Отправляет ответное сообщение, используя переданный в исходном
        /// сообщении обратный адрес и идентификатор сообщения.
        /// </summary>
        /// <typeparam name="TResponse">.NET тип отправляемого сообщения.</typeparam>
        /// <param name="response">Отправляемое сообщение.</param>
        /// <param name="expires">Настройки, которые определяют время пока ответ актуален.</param>
        public void Reply<TResponse>(TResponse response, Expires expires = null) where TResponse : class
        {
            var headers = new Dictionary<string, object>();

            if (expires != null)
            {
                headers[Headers.Expires] = expires.ToString();
            }

            this.Delivery.ReplyWith(new Message<TResponse>(MessageLabel.Empty, headers, response));
        }
    }
}
