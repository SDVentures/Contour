using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

using Contour.Testing.Transport.RabbitMq;

using FluentAssertions;

using Contour.Validation;

using NUnit.Framework;
using FluentAssertions.Extensions;

namespace Contour.RabbitMq.Tests
{
    /// <summary>
    /// The validation specs.
    /// </summary>
    // ReSharper disable InconsistentNaming
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
    public class ValidationSpecs
    {
        /// <summary>
        /// The boo validator.
        /// </summary>
        public class BooValidator : IMessageValidatorOf<BooMessage>
        {
            public ValidationResult Validate(Message<BooMessage> message)
            {
                return Validate(message.Payload);
            }

            public ValidationResult Validate(IMessage message)
            {
                if (message.Payload is BooMessage)
                {
                    return Validate((BooMessage)message.Payload);
                }
                else
                {
                    return new ValidationResult(new BrokenRule("should be BooMessage"));
                }
            }

            private ValidationResult Validate(BooMessage booMessage)
            {
                if (booMessage.Num > 100)
                {
                    return ValidationResult.Valid;
                }
                else
                {
                    return new ValidationResult(new BrokenRule("Num"));
                }
            }
        }

        /// <summary>
        /// The foo validator.
        /// </summary>
        public class FooValidator : IMessageValidatorOf<FooMessage>
        {
            public ValidationResult Validate(Message<FooMessage> message)
            {
                return Validate(message.Payload);
            }

            public ValidationResult Validate(IMessage message)
            {
                if (message.Payload is FooMessage)
                {
                    return Validate((FooMessage)message.Payload);
                }
                else
                {
                    return new ValidationResult(new BrokenRule("should be BooMessage"));
                }
            }

            private ValidationResult Validate(FooMessage booMessage)
            {
                if (booMessage.Num < 100)
                {
                    return ValidationResult.Valid;
                }
                else
                {
                    return new ValidationResult(new BrokenRule("Num"));
                }
            }
        }

        /// <summary>
        /// The when_receiving_broken_message_with_composite_validator_set_on_bus.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_receiving_broken_message_with_composite_validator_set_on_bus : RabbitMqFixture
        {
            #region Public Methods and Operators

            /// <summary>
            /// The should_validate_according_to_payload_type.
            /// </summary>
            [Test]
            public void should_validate_according_to_payload_type()
            {
                var consumed = new AutoResetEvent(false);
                var failed = new AutoResetEvent(false);
                Exception exception = null;

                IBus producer = this.StartBus(
                    "producer",
                    cfg =>
                        {
                            cfg.Route("boo");
                            cfg.Route("foo");
                        });

                this.StartBus(
                    "consumer",
                    cfg =>
                        {
                            cfg.RegisterValidators(new MessageValidatorGroup(new IMessageValidator[] { new BooValidator(), new FooValidator() }));

                            cfg.OnFailed(
                                ctx =>
                                    {
                                        exception = ctx.Exception;
                                        failed.Set();
                                        ctx.Accept();
                                    });

                            cfg.On<BooMessage>("boo").
                                ReactWith((m, ctx) => consumed.Set());
                            cfg.On<FooMessage>("foo").
                                ReactWith((m, ctx) => consumed.Set());
                        });

                producer.Emit("boo", new BooMessage(13));

                consumed.WaitOne(3.Seconds()).Should().BeFalse();
                failed.WaitOne(3.Seconds()).Should().BeTrue();
                exception.Should().BeOfType<MessageValidationException>();
                exception.Message.Should().Contain("Num");

                producer.Emit("foo", new FooMessage(130));

                consumed.WaitOne(3.Seconds()).Should().BeFalse();
                failed.WaitOne(3.Seconds()).Should().BeTrue();
                exception.Should().BeOfType<MessageValidationException>();
                exception.Message.Should().Contain("Num");
            }

            #endregion
        }

        /// <summary>
        /// The when_receiving_broken_message_with_validator_set_on_bus.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_receiving_broken_message_with_validator_set_on_bus : RabbitMqFixture
        {
            #region Public Methods and Operators

            /// <summary>
            /// The should_not_invoke_consumer.
            /// </summary>
            [Test]
            public void should_not_invoke_consumer()
            {
                var consumed = new AutoResetEvent(false);
                var failed = new AutoResetEvent(false);
                Exception exception = null;

                IBus producer = this.StartBus("producer", cfg => cfg.Route("boo"));

                this.StartBus(
                    "consumer",
                    cfg =>
                        {
                            cfg.RegisterValidator(new BooValidator());

                            cfg.On<BooMessage>("boo").
                                ReactWith((m, ctx) => consumed.Set()).
                                OnFailed(
                                    ctx =>
                                        {
                                            exception = ctx.Exception;
                                            failed.Set();
                                            ctx.Accept();
                                        });
                        });

                producer.Emit("boo", new BooMessage(13));

                consumed.WaitOne(3.Seconds()).Should().BeFalse();
                failed.WaitOne(3.Seconds()).Should().BeTrue();
                exception.Should().BeOfType<MessageValidationException>();
                exception.Message.Should().Contain("Num");
            }

            #endregion
        }

        /// <summary>
        /// The when_receiving_broken_message_with_validator_set_on_subscription.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_receiving_broken_message_with_validator_set_on_subscription : RabbitMqFixture
        {
            #region Public Methods and Operators

            /// <summary>
            /// The should_not_invoke_consumer.
            /// </summary>
            [Test]
            public void should_not_invoke_consumer()
            {
                var consumed = new AutoResetEvent(false);
                var failed = new AutoResetEvent(false);
                Exception exception = null;

                IBus producer = this.StartBus("producer", cfg => cfg.Route("boo"));

                this.StartBus(
                    "consumer",
                    cfg => cfg.On<BooMessage>("boo").
                               ReactWith((m, ctx) => consumed.Set()).
                               WhenVerifiedBy(new BooValidator()).
                               OnFailed(
                                   ctx =>
                                       {
                                           exception = ctx.Exception;
                                           failed.Set();
                                           ctx.Accept();
                                       }));

                producer.Emit("boo", new BooMessage(13));

                consumed.WaitOne(3.Seconds()).
                    Should().
                    BeFalse();
                failed.WaitOne(3.Seconds()).
                    Should().
                    BeTrue();
                exception.Should().
                    BeOfType<MessageValidationException>();
                exception.Message.Should().
                    Contain("Num");
            }

            #endregion
        }

        /// <summary>
        /// The when_receiving_reply_message_with_validator_set_on_bus.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_receiving_reply_message_with_validator_set_on_bus : RabbitMqFixture
        {
            #region Public Methods and Operators

            /// <summary>
            /// The should_not_invoke_response_action.
            /// </summary>
            [Test]
            public void should_not_invoke_response_action()
            {
                var consumed = new AutoResetEvent(false);
                var failed = new AutoResetEvent(false);
                Exception exception = null;

                IBus producer = this.StartBus(
                    "producer",
                    cfg =>
                        {
                            cfg.RegisterValidator(new BooValidator());

                            cfg.OnFailed(
                                ctx =>
                                    {
                                        exception = ctx.Exception;
                                        failed.Set();
                                        ctx.Accept();
                                    });

                            cfg.Route("request").
                                WithDefaultCallbackEndpoint();
                        });

                this.StartBus(
                    "consumer",
                    cfg => cfg.On<DummyRequest>("request").
                               ReactWith((m, ctx) => ctx.Reply(new BooMessage(m.Num))));

                producer.RequestAsync<DummyRequest, BooMessage>("request", new DummyRequest(13)).
                    ContinueWith(
                        t =>
                            {
                                if (!t.IsFaulted)
                                {
                                    consumed.Set();
                                }
                            });

                consumed.WaitOne(3.Seconds()).
                    Should().
                    BeFalse();
                failed.WaitOne(3.Seconds()).
                    Should().
                    BeTrue();
                exception.Should().
                    BeOfType<MessageValidationException>();
                exception.Message.Should().
                    Contain("Num");
            }

            #endregion
        }
    }

    // ReSharper restore InconsistentNaming
}
