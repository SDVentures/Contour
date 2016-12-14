namespace Contour.Validation
{
    /// <summary>
    /// Represents a message validator collection
    /// </summary>
    public interface IMessageValidatorRegistry
    {
        /// <summary>
        /// Registers a group of validators
        /// </summary>
        /// <param name="validatorGroup"></param>
        void Register(MessageValidatorGroup validatorGroup);

        /// <summary>
        /// Registers a validator
        /// </summary>
        /// <param name="validator"></param>
        void Register(IMessageValidator validator);

        /// <summary>
        /// Registers a validator
        /// </summary>
        /// <param name="validator"></param>
        /// <typeparam name="T"></typeparam>
        void Register<T>(IMessageValidatorOf<T> validator) where T : class;

        /// <summary>
        /// Validates a message using a collection of validators
        /// </summary>
        /// <param name="message"></param>
        void Validate(IMessage message);
    }
}