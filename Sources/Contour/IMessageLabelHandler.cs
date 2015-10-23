namespace Contour
{
    /// <summary>
    /// Определяет и встаивает метку сообщения.
    /// </summary>
    public interface IMessageLabelHandler
    {
        /// <summary>
        /// Встраивает метку сообщения.
        /// </summary>
        /// <param name="raw">Объект, куда внедряется метка сообщения.</param>
        /// <param name="label">Встраиваемая метка сообщения.</param>
        void Inject(object raw, MessageLabel label);

        /// <summary>
        /// Определяет метку сообщения по объекту.
        /// </summary>
        /// <param name="raw">Объект, на основе которого определяется метка сообщения.</param>
        /// <returns>Метка сообщения.</returns>
        MessageLabel Resolve(object raw);
    }
}
