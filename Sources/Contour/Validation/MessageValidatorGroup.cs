namespace Contour.Validation
{
    using System.Collections.Generic;
    using System.Linq;

    using Contour.Helpers.CodeContracts;

    /// <summary>
    /// The message validator group.
    /// </summary>
    public sealed class MessageValidatorGroup
    {
        /// <summary>
        /// The _validators.
        /// </summary>
        private readonly IList<IMessageValidator> _validators;
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="MessageValidatorGroup"/>.
        /// </summary>
        /// <param name="validators">
        /// The validators.
        /// </param>
        public MessageValidatorGroup(IEnumerable<IMessageValidator> validators)
        {
            Requires.NotNull(validators, "validators");

            this._validators = validators.ToList();
        }
        /// <summary>
        /// Gets the validators.
        /// </summary>
        public IEnumerable<IMessageValidator> Validators
        {
            get
            {
                return this._validators;
            }
        }
    }
}
