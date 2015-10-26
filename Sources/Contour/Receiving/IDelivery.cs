using System;
using System.Threading.Tasks;

namespace Contour.Receiving
{
    /// <summary>
    /// Доставленное сообщение.
    /// </summary>
    public interface IDelivery
    {
        /// <summary>
        /// Канал доставки сообщения.
        /// </summary>
        IChannel Channel { get; }

        /// <summary>
        /// Метка доставленных сообщений.
        /// </summary>
        MessageLabel Label { get; }

        /// <summary>
        /// Маршрут ответа на доставленное сообщение.
        /// </summary>
        IRoute ReplyRoute { get; }

        /// <summary>
        /// Верно, если можно ответить на это сообщение.
        /// </summary>
        bool CanReply { get; }

        /// <summary>
        /// Подтверждает доставку сообщения.
        /// </summary>
        void Accept();

        /// <summary>
        /// Пересылает сообщение, устанавливая указанную метку.
        /// </summary>
        /// <param name="label">Новая метка, с которой пересылается сообщение.</param>
        /// <param name="payload">Новое содержимое сообщения.</param>
        /// <returns>Задача пересылки сообщения.</returns>
        Task Forward(MessageLabel label, object payload);

        /// <summary>
        /// Помечает сообщение как необработанное.
        /// </summary>
        /// <param name="requeue">
        /// Сообщение требуется вернуть во входящую очередь для повторной обработки.
        /// </param>
        void Reject(bool requeue);

        /// <summary>
        /// Отсылает ответное сообщение.
        /// </summary>
        /// <param name="message">Ответное сообщение.</param>
        void ReplyWith(IMessage message);

        /// <summary>
        /// Конвертирует полученную информацию в сообщение указанного типа.
        /// </summary>
        /// <param name="type">Тип сообщения.</param>
        /// <returns>Сообщение указанного типа.</returns>
        IMessage UnpackAs(Type type);
    }
}
