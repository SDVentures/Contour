namespace Contour.Validation
{
    using System;
    using System.Collections.Generic;

    using Contour.Helpers;
    using Contour.Helpers.CodeContracts;

    /// <summary>
    /// The message validator registry.
    /// </summary>
    internal class MessageValidatorRegistry
    {
        #region Fields

        /// <summary>
        /// The _validators.
        /// </summary>
        private readonly IDictionary<Type, IMessageValidator> _validators = new Dictionary<Type, IMessageValidator>();

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The register.
        /// </summary>
        /// <param name="validator">
        /// The validator.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        public void Register<T>(IMessageValidatorOf<T> validator) where T : class
        {
            this._validators[typeof(T)] = validator;
        }

        /// <summary>
        /// The register.
        /// </summary>
        /// <param name="validatorGroup">
        /// The validator group.
        /// </param>
        public void Register(MessageValidatorGroup validatorGroup)
        {
            Requires.NotNull(validatorGroup, "validatorGroup");

            validatorGroup.Validators.ForEach(Register);
        }

        /// <summary>
        /// The register.
        /// </summary>
        /// <param name="validator">
        /// The validator.
        /// </param>
        public void Register(IMessageValidator validator)
        {
            IList<Type> supportedTypes = Reflection.GetGenericTypeParameterOf(typeof(IMessageValidatorOf<>), validator);

            supportedTypes.ForEach(t => this._validators[t] = validator);
        }

        /// <summary>
        /// The validate.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public void Validate(IMessage message)
        {
            // TODO: disallow using nulls
            if (message.Payload == null)
            {
                return;
            }

            Type type = message.Payload.GetType();
            IMessageValidator validator;
            if (!this._validators.TryGetValue(type, out validator))
            {
                return;
            }

            validator.Validate(message).
                ThrowIfBroken();
        }

        #endregion
    }
}
