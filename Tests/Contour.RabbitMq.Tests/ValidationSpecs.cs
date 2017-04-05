using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

using Contour.Testing.Transport.RabbitMq;

using FluentAssertions;

using FluentValidation;

using Contour.Validation;
using Contour.Validation.Fluent;

using NUnit.Framework;

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
        public class BooValidator : FluentPayloadValidatorOf<BooMessage>
        {
            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="BooValidator"/>.
            /// </summary>
            public BooValidator()
            {
                this.RuleFor(x => x.Num).
                    GreaterThan(100);
            }
        }

        /// <summary>
        /// The foo validator.
        /// </summary>
        public class FooValidator : FluentPayloadValidatorOf<FooMessage>
        {
            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="FooValidator"/>.
            /// </summary>
            public FooValidator()
            {
                this.RuleFor(x => x.Num).
                    LessThan(100);
            }
        }

        /// <summary>
        /// The when_receiving_broken_message_with_composite_validator_set_on_bus.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_receiving_broken_message_with_composite_validator_set_on_bus : RabbitMqFixture
        {
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

                producer.Emit("foo", new FooMessage(130));

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
        }

        /// <summary>
        /// The when_receiving_broken_message_with_validator_set_on_bus.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_receiving_broken_message_with_validator_set_on_bus : RabbitMqFixture
        {
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
        }

        /// <summary>
        /// The when_receiving_broken_message_with_validator_set_on_subscription.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_receiving_broken_message_with_validator_set_on_subscription : RabbitMqFixture
        {
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
        }

        /// <summary>
        /// The when_receiving_reply_message_with_validator_set_on_bus.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_receiving_reply_message_with_validator_set_on_bus : RabbitMqFixture
        {
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
        }
    }

    // ReSharper restore InconsistentNaming
}
