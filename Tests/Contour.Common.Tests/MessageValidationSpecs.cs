namespace Contour.Common.Tests
{
    using System.Dynamic;
    using System.Linq;

    using FluentAssertions;

    using FluentValidation;

    using Contour.Validation;
    using Contour.Validation.Fluent;

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
        public class BooValidator : FluentPayloadValidatorOf<Boo>
        {
            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="BooValidator"/>.
            /// </summary>
            public BooValidator()
            {
                this.RuleFor(x => x.Num).
                    GreaterThan(10);
                this.RuleFor(x => x.Str).
                    NotEmpty();
            }
        }

        /// <summary>
        /// The dynamic validator.
        /// </summary>
        public class DynamicValidator : FluentPayloadValidatorOf<dynamic>
        {
            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="DynamicValidator"/>.
            /// </summary>
            public DynamicValidator()
            {
                this.RuleFor(x => x).
                    Must(x => x.Num > 10).
                    WithName("Num");
            }
        }

        /// <summary>
        /// The expando validator.
        /// </summary>
        public class ExpandoValidator : FluentPayloadValidatorOf<ExpandoObject>
        {
            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="ExpandoValidator"/>.
            /// </summary>
            public ExpandoValidator()
            {
                this.RuleFor(x => (dynamic)x).
                    Must(x => x.Num > 10).
                    WithName("Num");
            }
        }

        /// <summary>
        /// The when_registering_validator_in_registry.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class when_registering_validator_in_registry
        {
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
                    ShouldThrow<MessageValidationException>();
            }
        }

        /// <summary>
        /// The when_registering_validator_in_registry_if_validator_already_present.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class when_registering_validator_in_registry_if_validator_already_present
        {
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
                    ShouldNotThrow();

                stubValidator.Verify(v => v.Validate(It.IsAny<IMessage>()), Times.Once);
            }
        }

        /// <summary>
        /// The when_throwing_on_invalid_result.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class when_throwing_on_invalid_result
        {
            /// <summary>
            /// The should_throw.
            /// </summary>
            [Test]
            public void should_throw()
            {
                var validationResult = new ValidationResult(new[] { new BrokenRule("Something is broken") });

                validationResult.Invoking(r => r.ThrowIfBroken()).
                    ShouldThrow<MessageValidationException>();
            }
        }

        /// <summary>
        /// The when_throwing_on_valid_result.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class when_throwing_on_valid_result
        {
            /// <summary>
            /// The should_not_throw.
            /// </summary>
            [Test]
            public void should_not_throw()
            {
                ValidationResult validationResult = ValidationResult.Valid;
                validationResult.Invoking(r => r.ThrowIfBroken()).
                    ShouldNotThrow();
            }
        }

        /// <summary>
        /// The when_validating_message_of_concrete_class_with_invalid_data.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class when_validating_message_of_concrete_class_with_invalid_data
        {
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
        }

        /// <summary>
        /// The when_validating_message_of_concrete_class_with_valid_data.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class when_validating_message_of_concrete_class_with_valid_data
        {
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
        }

        /// <summary>
        /// The when_validating_message_using_dynamic_validator.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class when_validating_message_using_dynamic_validator
        {
            /// <summary>
            /// The should_validate.
            /// </summary>
            [Test]
            public void should_validate()
            {
                var validator = new DynamicValidator();

                var payload = new { Num = 3, Str = "this" };
                ValidationResult result = validator.Validate(new Message<dynamic>("label".ToMessageLabel(), payload));
                result.IsValid.Should().
                    BeFalse();
                result.BrokenRules.Single().
                    Description.Should().
                    Contain("Num");

                payload = new { Num = 13, Str = "this" };
                result = validator.Validate(new Message<dynamic>("label".ToMessageLabel(), payload));
                result.IsValid.Should().
                    BeTrue();
            }
        }

        /// <summary>
        /// The when_validating_message_using_expando_validator.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class when_validating_message_using_expando_validator
        {
            /// <summary>
            /// The should_validate.
            /// </summary>
            [Test]
            public void should_validate()
            {
                dynamic payload = new ExpandoObject();
                payload.Num = 3;
                payload.Str = "this";

                var validator = new ExpandoValidator();

                ValidationResult result = validator.Validate(new Message<ExpandoObject>("label".ToMessageLabel(), payload));
                result.IsValid.Should().
                    BeFalse();
                result.BrokenRules.Single().
                    Description.Should().
                    Contain("Num");

                payload.Num = 13;
                result = validator.Validate(new Message<ExpandoObject>("label".ToMessageLabel(), payload));
                result.IsValid.Should().
                    BeTrue();
            }
        }
    }

    // ReSharper restore InconsistentNaming
}
