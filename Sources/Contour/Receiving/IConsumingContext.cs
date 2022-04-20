using System.Threading.Tasks;

namespace Contour.Receiving
{
    /// <summary>
    /// Контекст обработки полученного сообщения.
    /// </summary>
    /// <typeparam name="T">Тип полученного сообщения.</typeparam>
    public interface IConsumingContext<T>
        where T : class
    {
        /// <summary>
        /// Конечная точка, в которой зарегистрирован текущий обработчик сообщения.
        /// </summary>
        IBusContext Bus { get; }

        /// <summary>
        /// Полученное сообщение.
        /// </summary>
        Message<T> Message { get; }

        /// <summary>
        /// Верно, если можно ответить на это сообщение.
        /// </summary>
        bool CanReply { get; }

        /// <summary>
        /// Помечает сообщение как обработанное.
        /// </summary>
        void Accept();

        /// <summary>
        /// Пересылает сообщение, устанавливая указанную метку.
        /// </summary>
        /// <param name="label">Новая метка, с которой пересылается сообщение.</param>
        void Forward(MessageLabel label);

        /// <summary>
        /// Пересылает сообщение, устанавливая указанную метку.
        /// </summary>
        /// <param name="label">Новая метка, с которой пересылается сообщение.</param>
        void Forward(string label);

        /// <summary>
        /// Пересылает сообщение, устанавливая указанную метку.
        /// </summary>
        /// <param name="label">Новая метка, с которой пересылается сообщение.</param>
        Task ForwardAsync(string label);
        
        /// <summary>
        /// Пересылает сообщение, устанавливая указанную метку.
        /// </summary>
        /// <param name="label">Новая метка, с которой пересылается сообщение.</param>
        /// <param name="payload">Новое содержимое сообщения.</param>
        Task ForwardAsync<TOut>(string label, TOut payload) where TOut : class;

        /// <summary>
        /// Пересылает сообщение, устанавливая указанную метку.
        /// </summary>
        /// <param name="label">Новая метка, с которой пересылается сообщение.</param>
        /// <param name="payload">Новое содержимое сообщения.</param>
        /// <typeparam name="TOut">Тип сообщения.</typeparam>
        void Forward<TOut>(MessageLabel label, TOut payload = null) where TOut : class;

        /// <summary>
        /// Пересылает сообщение, устанавливая указанную метку.
        /// </summary>
        /// <param name="label">Новая метка, с которой пересылается сообщение.</param>
        /// <param name="payload">Новое содержимое сообщения.</param>
        /// <typeparam name="TOut">Тип сообщения.</typeparam>
        void Forward<TOut>(string label, TOut payload = null) where TOut : class;

        /// <summary>
        /// Помечает сообщение как необработанное.
        /// </summary>
        /// <param name="requeue">
        /// Сообщение требуется вернуть во входящую очередь для повторной обработки.
        /// </param>
        void Reject(bool requeue);

        /// <summary>
        /// Отправляет ответное сообщение, используя переданный в исходном
        /// сообщении обратный адрес и идентификатор сообщения.
        /// </summary>
        /// <typeparam name="TResponse">.NET тип отправляемого сообщения.</typeparam>
        /// <param name="response">Отправляемое сообщение.</param>
        /// <param name="expires">Настройки, которые определяют время пока ответ актуален.</param>
        void Reply<TResponse>(TResponse response, Expires expires = null) where TResponse : class;
    }
}
