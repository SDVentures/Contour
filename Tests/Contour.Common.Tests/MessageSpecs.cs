namespace Contour.Common.Tests
{
    using System;
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
        private static readonly MessageProperties DefaultProperties = new MessageProperties();

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
                message.Properties.ShouldBeEquivalentTo(DefaultProperties);
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
                target.Properties.ShouldBeEquivalentTo(DefaultProperties);
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
                target.Properties.ShouldBeEquivalentTo(DefaultProperties);
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
                message.Properties.ShouldBeEquivalentTo(DefaultProperties);
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
                target.Properties.ShouldBeEquivalentTo(DefaultProperties);
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
                target.Properties.ShouldBeEquivalentTo(DefaultProperties);
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class when_creating_message_with_properties
        {
            [Test]
            public void should_create_untyped_message_with_defined_properties()
            {
                var message = new Message(
                    "boo".ToMessageLabel(),
                    new Dictionary<string, object> { { "This", "That" } },
                    "Body",
                    new MessageProperties(new DateTime(1970, 1, 1, 0, 0, 0)));

                message.Label.Name.Should().Be("boo");
                message.Headers.Should().HaveCount(1);
                message.Headers.Should().ContainKey("This");
                message.Payload.Should().Be("Body");
                message.Properties.Timestamp.Should().Be(new DateTime(1970, 1, 1, 0, 0, 0));
            }

            [Test]
            public void should_create_typed_message_with_defined_properties()
            {
                var message = new Message<string>(
                    "boo".ToMessageLabel(),
                    new Dictionary<string, object> { { "This", "That" } },
                    "Body",
                    new MessageProperties(new DateTime(1970, 1, 1, 0, 0, 0)));

                message.Should().BeOfType<Message<string>>();
                message.Label.Name.Should().Be("boo");
                message.Headers.Should().HaveCount(1);
                message.Headers.Should().ContainKey("This");
                message.Payload.Should().Be("Body");
                message.Properties.Timestamp.Should().Be(new DateTime(1970, 1, 1, 0, 0, 0));
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class when_cloning_message_with_properties
        {
            [Test]
            public void should_create_untyped_message_with_defined_properties_and_payload()
            {
                var original = new Message(
                    "boo".ToMessageLabel(),
                    new Dictionary<string, object> { { "This", "That" } },
                    "Body",
                    new MessageProperties(new DateTime(1970, 1, 1, 0, 0, 0)));

                var target = original.WithPayload("Zzzz");

                target.Label.Name.Should().Be("boo");
                target.Headers.Should().HaveCount(1);
                target.Headers.Should().ContainKey("This");
                target.Payload.Should().Be("Zzzz");
                target.Properties.Timestamp.Should().Be(new DateTime(1970, 1, 1, 0, 0, 0));
            }

            [Test]
            public void should_create_typed_message_with_defined_properties_and_payload()
            {
                var original = new Message<string>(
                    "boo".ToMessageLabel(),
                    new Dictionary<string, object> { { "This", "That" } },
                    "Body",
                    new MessageProperties(new DateTime(1970, 1, 1, 0, 0, 0)));

                var target = original.WithPayload("Zzzz");

                target.Should().BeOfType<Message<string>>();
                target.Label.Name.Should().Be("boo");
                target.Headers.Should().HaveCount(1);
                target.Headers.Should().ContainKey("This");
                target.Payload.Should().Be("Zzzz");
                target.Properties.Timestamp.Should().Be(new DateTime(1970, 1, 1, 0, 0, 0));
            }

            [Test]
            public void should_create_untyped_message_with_defined_properties_and_label()
            {
                var original = new Message(
                    "boo".ToMessageLabel(),
                    new Dictionary<string, object> { { "This", "That" } },
                    "Body",
                    new MessageProperties(new DateTime(1970, 1, 1, 0, 0, 0)));

                var target = original.WithLabel("moo".ToMessageLabel());

                target.Label.Name.Should().Be("moo");
                target.Headers.Should().HaveCount(1);
                target.Headers.Should().ContainKey("This");
                target.Payload.Should().Be("Body");
                target.Properties.Timestamp.Should().Be(new DateTime(1970, 1, 1, 0, 0, 0));
            }

            [Test]
            public void should_create_typed_message_with_defined_properties_and_label()
            {
                var original = new Message<string>(
                    "boo".ToMessageLabel(),
                    new Dictionary<string, object> { { "This", "That" } },
                    "Body",
                    new MessageProperties(new DateTime(1970, 1, 1, 0, 0, 0)));

                var target = original.WithLabel("moo".ToMessageLabel());

                target.Should().BeOfType<Message<string>>();
                target.Label.Name.Should().Be("moo");
                target.Headers.Should().HaveCount(1);
                target.Headers.Should().ContainKey("This");
                target.Payload.Should().Be("Body");
                target.Properties.Timestamp.Should().Be(new DateTime(1970, 1, 1, 0, 0, 0));
            }
        }
    }

    // ReSharper restore InconsistentNaming
}
