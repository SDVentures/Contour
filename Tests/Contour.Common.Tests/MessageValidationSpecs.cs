namespace Contour.Common.Tests
{
    using System.Dynamic;
    using System.Linq;

    using FluentAssertions;

    using Contour.Validation;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// The message validation specs.
    /// </summary>
    // ReSharper disable InconsistentNaming
    public class MessageValidationSpecs
    {
        /// <summary>
        /// The boo.
        /// </summary>
        public class Boo
        {
            /// <summary>
            /// Gets or sets the num.
            /// </summary>
            public int Num { get; set; }

            /// <summary>
            /// Gets or sets the str.
            /// </summary>
            public string Str { get; set; }
        }

        /// <summary>
        /// The boo validator.
        /// </summary>
        public class BooValidator : IMessageValidatorOf<Boo>
        {
            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="BooValidator"/>.
            /// </summary>
            public BooValidator()
            {
            }

            public ValidationResult Validate(Message<Boo> message)
            {
                return Validate(message.Payload);
            }

            public ValidationResult Validate(IMessage message)
            {
                if (message.Payload is Boo)
                {
                    return Validate((Boo)message.Payload);
                }
                else
                {
                    return new ValidationResult(new BrokenRule("should be type Boo"));
                }
            }

            private ValidationResult Validate(Boo boo)
            {
                if (boo.Num <= 10)
                {
                    return new ValidationResult(new BrokenRule("should be greater than 10"));
                }
                else if (string.IsNullOrEmpty(boo.Str))
                {
                    return new ValidationResult(new BrokenRule("Str"));
                }
                else
                {
                    return ValidationResult.Valid;
                }
            }
        }

        /// <summary>
        /// The when_registering_validator_in_registry.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class when_registering_validator_in_registry
        {
            #region Public Methods and Operators

            /// <summary>
            /// The should_use_registered_validator.
            /// </summary>
            [Test]
            public void should_use_registered_validator()
            {
                var registry = new MessageValidatorRegistry();

                registry.Register(new BooValidator());

                var payload = new Boo { Num = 3, Str = "this" };

                registry.Invoking(r => r.Validate(new Message<Boo>("label".ToMessageLabel(), payload))).
                    Should().Throw<MessageValidationException>();
            }

            #endregion
        }

        /// <summary>
        /// The when_registering_validator_in_registry_if_validator_already_present.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class when_registering_validator_in_registry_if_validator_already_present
        {
            #region Public Methods and Operators

            /// <summary>
            /// The should_replace_validator.
            /// </summary>
            [Test]
            public void should_replace_validator()
            {
                var registry = new MessageValidatorRegistry();

                registry.Register(new BooValidator());

                var stubValidator = new Mock<IMessageValidatorOf<Boo>>();
                stubValidator.Setup(v => v.Validate(It.IsAny<IMessage>())).
                    Returns(ValidationResult.Valid);

                registry.Register(stubValidator.Object);

                var payload = new Boo { Num = 3, Str = "this" };

                registry.Invoking(r => r.Validate(new Message<Boo>("label".ToMessageLabel(), payload))).
                    Should().NotThrow();

                stubValidator.Verify(v => v.Validate(It.IsAny<IMessage>()), Times.Once);
            }

            #endregion
        }

        /// <summary>
        /// The when_throwing_on_invalid_result.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class when_throwing_on_invalid_result
        {
            #region Public Methods and Operators

            /// <summary>
            /// The should_throw.
            /// </summary>
            [Test]
            public void should_throw()
            {
                var validationResult = new ValidationResult(new[] { new BrokenRule("Something is broken") });

                validationResult.Invoking(r => r.ThrowIfBroken()).
                    Should().Throw<MessageValidationException>();
            }

            #endregion
        }

        /// <summary>
        /// The when_throwing_on_valid_result.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class when_throwing_on_valid_result
        {
            #region Public Methods and Operators

            /// <summary>
            /// The should_not_throw.
            /// </summary>
            [Test]
            public void should_not_throw()
            {
                ValidationResult validationResult = ValidationResult.Valid;
                validationResult.Invoking(r => r.ThrowIfBroken()).
                    Should().NotThrow();
            }

            #endregion
        }

        /// <summary>
        /// The when_validating_message_of_concrete_class_with_invalid_data.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class when_validating_message_of_concrete_class_with_invalid_data
        {
            #region Public Methods and Operators

            /// <summary>
            /// The should_validate.
            /// </summary>
            [Test]
            public void should_validate()
            {
                var payload = new Boo { Num = 13, Str = string.Empty };

                var validator = new BooValidator();

                ValidationResult result = validator.Validate(new Message<Boo>("label".ToMessageLabel(), payload));

                result.IsValid.Should().
                    BeFalse();
                result.BrokenRules.Should().
                    HaveCount(1);
                result.BrokenRules.First().
                    Description.Should().
                    Contain("Str");
            }

            #endregion
        }

        /// <summary>
        /// The when_validating_message_of_concrete_class_with_valid_data.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class when_validating_message_of_concrete_class_with_valid_data
        {
            #region Public Methods and Operators

            /// <summary>
            /// The should_validate.
            /// </summary>
            [Test]
            public void should_validate()
            {
                var payload = new Boo { Num = 13, Str = "this" };

                var validator = new BooValidator();

                ValidationResult result = validator.Validate(new Message<Boo>("label".ToMessageLabel(), payload));

                result.IsValid.Should().
                    BeTrue();
            }

            #endregion
        }
    }

    // ReSharper restore InconsistentNaming
}
