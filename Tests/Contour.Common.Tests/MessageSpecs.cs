namespace Contour.Common.Tests
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    using FluentAssertions;

    using NUnit.Framework;

    /// <summary>
    /// The message specs.
    /// </summary>
    // ReSharper disable InconsistentNaming
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
    public class MessageSpecs
    {
        [TestFixture]
        [Category("Unit")]
        public class when_creating_untyped_message
        {
            [Test]
            public void should_create_message_with_defined_state()
            {
                var message = new Message(
                    "boo".ToMessageLabel(),
                    new Dictionary<string, object> { { "This", "That" } },
                    "Body");

                message.Label.Name.Should().Be("boo");
                message.Headers.Should().HaveCount(1);
                message.Headers.Should().ContainKey("This");
                message.Payload.Should().Be("Body");
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class when_cloning_untyped_message_with_new_label
        {
            [Test]
            public void should_create_message_with_combined_state()
            {
                var original = new Message(
                    "boo".ToMessageLabel(),
                    new Dictionary<string, object> { { "This", "That" } },
                    "Body");

                var target = original.WithLabel("moo".ToMessageLabel());

                target.Should().BeOfType<Message>();
                target.Label.Name.Should().Be("moo");
                target.Headers.Should().HaveCount(1);
                target.Headers.Should().ContainKey("This");
                target.Payload.Should().Be("Body");
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class when_cloning_untyped_message_with_new_payload
        {
            [Test]
            public void should_create_message_with_combined_state()
            {
                var original = new Message(
                    "boo".ToMessageLabel(),
                    new Dictionary<string, object> { { "This", "That" } },
                    "Body");

                var target = original.WithPayload("Zzzz");

                target.Should().BeOfType<Message>();
                target.Label.Name.Should().Be("boo");
                target.Headers.Should().HaveCount(1);
                target.Headers.Should().ContainKey("This");
                target.Payload.Should().Be("Zzzz");
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class when_creating_typed_message
        {
            [Test]
            public void should_create_message_with_defined_state()
            {
                var message = new Message<string>(
                    "boo".ToMessageLabel(),
                    new Dictionary<string, object> { { "This", "That" } },
                    "Body");

                message.Label.Name.Should().Be("boo");
                message.Headers.Should().HaveCount(1);
                message.Headers.Should().ContainKey("This");
                message.Payload.Should().Be("Body");
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class when_cloning_typed_message_with_new_label
        {
            [Test]
            public void should_create_message_with_combined_state()
            {
                var original = new Message<string>(
                    "boo".ToMessageLabel(),
                    new Dictionary<string, object> { { "This", "That" } },
                    "Body");

                var target = original.WithLabel("moo".ToMessageLabel());

                target.Should().BeOfType<Message<string>>();
                target.Label.Name.Should().Be("moo");
                target.Headers.Should().HaveCount(1);
                target.Headers.Should().ContainKey("This");
                target.Payload.Should().Be("Body");
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class when_cloning_typed_message_with_new_payload
        {
            [Test]
            public void should_create_message_with_combined_state()
            {
                var original = new Message<string>(
                    "boo".ToMessageLabel(),
                    new Dictionary<string, object> { { "This", "That" } },
                    "Body");

                var target = original.WithPayload("Zzzz");

                target.Should().BeOfType<Message<string>>();
                target.Label.Name.Should().Be("boo");
                target.Headers.Should().HaveCount(1);
                target.Headers.Should().ContainKey("This");
                target.Payload.Should().Be("Zzzz");
            }
        }
    }

    // ReSharper restore InconsistentNaming
}
