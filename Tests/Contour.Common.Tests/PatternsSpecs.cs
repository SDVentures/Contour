using Contour.Receiving;

using Moq;

namespace Contour.Common.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;

    using FluentAssertions;

    using Contour.Operators;

    using NUnit.Framework;

    /// <summary>
    /// The message specs.
    /// </summary>
    // ReSharper disable InconsistentNaming
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
    public class PatternsSpecs
    {
        [TestFixture]
        [Category("Unit")]
        public class when_applying_filtering_to_message
        {
            [Test]
            public void should_filter_based_on_expression()
            {
                var processor = new Filter(m => m.Headers.ContainsKey("This") || ((string)m.Payload).Contains("!"));

                var message = new Message(
                    "boo".ToMessageLabel(),
                    new Dictionary<string, object> { { "This", "That" } },
                    "Body");

                var result = processor.Apply(message).ToList();

                result.Should().HaveCount(1);
                result.Single().Label.Name.Should().Be("boo");

                message = new Message(
                    "boo".ToMessageLabel(),
                    new Dictionary<string, object>(),
                    "Bo!dy");

                result = processor.Apply(message).ToList();

                result.Should().HaveCount(1);
                result.Single().Label.Name.Should().Be("boo");

                message = new Message(
                    "boo".ToMessageLabel(),
                    new Dictionary<string, object>(),
                    "Body");

                result = processor.Apply(message).ToList();

                result.Should().BeEmpty();
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class when_applying_static_router_to_message
        {
            [Test]
            public void should_change_message_label()
            {
                var processor = new StaticRouter("other".ToMessageLabel());

                var message = new Message(
                    "boo".ToMessageLabel(),
                    new Dictionary<string, object> { { "This", "That" } },
                    "Body");

                var result = processor.Apply(message).ToList();

                result.Should().HaveCount(1);
                result.Single().Label.Name.Should().Be("other");
                result.Single().Payload.Should().Be("Body");
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class when_applying_content_router_to_message
        {
            [Test]
            public void should_change_message_label()
            {
                var processor = new ContentBasedRouter(m => string.Format("l-{0}", ((string)m.Payload).Length).ToMessageLabel());

                var message = new Message(
                    "boo".ToMessageLabel(),
                    new Dictionary<string, object> { { "This", "That" } },
                    "Body");

                var result = processor.Apply(message).ToList();

                result.Should().HaveCount(1);
                result.Single().Label.Name.Should().Be("l-4");
                result.Single().Payload.Should().Be("Body");

                message = new Message(
                    "boo".ToMessageLabel(),
                    new Dictionary<string, object> { { "This", "That" } },
                    "Another");

                result = processor.Apply(message).ToList();

                result.Should().HaveCount(1);
                result.Single().Label.Name.Should().Be("l-7");
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class when_applying_translator_to_message
        {
            [Test]
            public void should_change_message_payload()
            {
                var processor = new Translator(
                    m => string.Format("Got : {0}", ((Boo)m.Payload).A));

                var payload = new Boo { A = 10 };

                var message = new Message<Boo>(
                    "boo".ToMessageLabel(),
                    new Dictionary<string, object> { { "This", "That" } },
                    payload);

                var result = processor.Apply(message).ToList();

                result.Should().HaveCount(1);
                result.Single().Label.Name.Should().Be("boo");
                result.Single().Payload.Should().Be("Got : 10");
            }

            public class Boo
            {
                public int A { get; set; }

                public int B { get; set; }
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class when_applying_enricher_to_message
        {
            [Test]
            public void should_change_message_payload()
            {
                var processor = new Enricher(
                    m =>
                    {
                        var p = (Boo)m.Payload;
                        p.B = p.A * 2;
                    });

                var payload = new Boo { A = 10 };

                var message = new Message<Boo>(
                    "boo".ToMessageLabel(),
                    new Dictionary<string, object> { { "This", "That" } },
                    payload);

                var result = processor.Apply(message).ToList();

                result.Should().HaveCount(1);
                result.Single().Label.Name.Should().Be("boo");
                result.Single().Payload.Should().Be(payload);
                result.Single()
                    .Payload.As<Boo>()
                    .B.Should()
                    .Be(20);
            }

            public class Boo
            {
                public int A { get; set; }

                public int B { get; set; }
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class when_applying_splitter_to_message
        {
            [Test]
            public void should_produce_arbitrary_count_of_message()
            {
                var processor = new Splitter(
                    m =>
                    {
                        var p = (string)m.Payload;
                        return p
                            .ToCharArray()
                            .Select(c => c.ToString(CultureInfo.InvariantCulture));
                    });

                var message = new Message(
                    "boo".ToMessageLabel(),
                    new Dictionary<string, object> { { "This", "That" } },
                    "Body");

                var result = processor.Apply(message).ToList();

                result.Should().HaveCount(4);
                result.Should().OnlyContain(m => m.Label.Name == "boo");
                result.Should().OnlyContain(m => m.Headers.ContainsKey("This"));
                result.Should().Contain(m => (string)m.Payload == "B");
                result.Should().Contain(m => (string)m.Payload == "o");
                result.Should().Contain(m => (string)m.Payload == "d");
                result.Should().Contain(m => (string)m.Payload == "y");
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class when_applying_pipelined_operators_to_message
        {
            [Test]
            public void should_process_in_predefined_order()
            {
                var splitter = new Splitter(
                    m =>
                    {
                        var p = (string)m.Payload;
                        return p
                            .ToCharArray()
                            .Select(c => Tuple.Create(c));
                    });
                var filter = new Filter(m => char.IsLetter(((Tuple<char>)m.Payload).Item1));
                var translator = new Translator(m => char.IsLower(((Tuple<char>)m.Payload).Item1) ? "lower" : "upper");
                var router = new ContentBasedRouter(m => ((string)m.Payload + " route").ToMessageLabel());

                var processor = new Pipeline(
                    splitter, filter, translator, router);

                var message = new Message(
                    "boo".ToMessageLabel(),
                    new Dictionary<string, object> { { "This", "That" } },
                    "B1o-d,y");

                var result = processor.Apply(message).ToList();

                result.Should().HaveCount(4);
                result.Should().OnlyContain(m => m.Headers.ContainsKey("This"));

                result[0].Label.Name.Should().Be("upper route");
                result[1].Label.Name.Should().Be("lower route");
                result[2].Label.Name.Should().Be("lower route");
                result[3].Label.Name.Should().Be("lower route");

                result[0].Payload.Should().Be("upper");
                result[1].Payload.Should().Be("lower");
                result[2].Payload.Should().Be("lower");
                result[3].Payload.Should().Be("lower");
            }
        }

        /// <summary>
        /// Тестовая конфигурация для проверки оператора подтверждения обработки сообщения.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class when_applying_acceptor_to_message
        {
            /// <summary>
            /// Проверяет, что оператор подтверждает сообщение.
            /// </summary>
            [Test]
            public void should_accept_message()
            {
                var processor = new Acceptor();

                var message = new Message(
                    "boo".ToMessageLabel(),
                    new Dictionary<string, object> { { "This", "That" } },
                    "Body");

                Mock<IDelivery> deliveryMock = new Mock<IDelivery>();
                BusProcessingContext.Current = new BusProcessingContext(deliveryMock.Object);
                var result = processor.Apply(message).ToList();

                result.Should().HaveCount(1, "Сообщение должно быть передано дальше.");
                deliveryMock.Verify(dm => dm.Accept(), "Должна быть вызвана операция подтверждения обработки.");
            }
        }

        /// <summary>
        /// Тестовая конфигурация для проверки оператора ответа на запрос с передачей сообщения дальше.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class when_applying_transparent_reply
        {
            /// <summary>
            /// Проверяет, что оператор ответа на запрос передает сообщение дальше.
            /// </summary>
            [Test]
            public void should_pass_through_message()
            {
                var processor = new TransparentReply();

                var message = new Message(
                    "boo".ToMessageLabel(),
                    new Dictionary<string, object> { { "This", "That" } },
                    "Body");

                Mock<IDelivery> deliveryMock = new Mock<IDelivery>();
                BusProcessingContext.Current = new BusProcessingContext(deliveryMock.Object);
                var result = processor.Apply(message).ToList();

                result.Should().HaveCount(1, "Сообщение должно быть передано дальше.");
            }

            /// <summary>
            /// Проверяет, что оператор отвечает на запрос переданным сообщением.
            /// </summary>
            [Test]
            public void should_reply_by_message()
            {
                var processor = new TransparentReply();

                var message = new Message(
                    "boo".ToMessageLabel(),
                    new Dictionary<string, object> { { "This", "That" } },
                    "Body");

                Mock<IDelivery> deliveryMock = new Mock<IDelivery>();
                BusProcessingContext.Current = new BusProcessingContext(deliveryMock.Object);
                processor.Apply(message).ToList();

                deliveryMock.Verify(d => d.ReplyWith(It.IsAny<IMessage>()), "Должна быть вызвана отправка ответного сообщения.");
            }
        }

        /// <summary>
        /// Тестовая конфигурация для проверки оператора перенаправления сообщения динамическому списку получателей.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class when_applying_recipient_list
        {
            /// <summary>
            /// Проверяет, что оператор возвращает список сообщений в соответствии со списком получателей.
            /// </summary>
            [Test]
            public void should_pass_through_message()
            {
                var labels = new[]
                                 {
                                     "boo.r1".ToMessageLabel(),
                                     "boo.r2".ToMessageLabel(),
                                     "boo.r3".ToMessageLabel()
                                 };
                var processor = new RecipientList(m => labels);

                var message = new Message(
                    "boo".ToMessageLabel(),
                    new Dictionary<string, object> { { "This", "That" } },
                    "Body");

                var result = processor.Apply(message).ToList();

                result.Should().HaveCount(labels.Length, "Количество сообщений должно соответствовать полученному списку получателей.");
                result.Should().Contain(m => m.Label.ToString() == "boo.r1", "Должно быть сообщение c меткой boo.r1");
                result.Should().Contain(m => m.Label.ToString() == "boo.r2", "Должно быть сообщение c меткой boo.r2");
                result.Should().Contain(m => m.Label.ToString() == "boo.r3", "Должно быть сообщение c меткой boo.r3");
            }
        }

        /// <summary>
        /// Тестовая конфигурация для проверки оператора инспеции сообщений в шине.
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class when_applying_wire_tap
        {
            /// <summary>
            /// Проверяет, что оператор возвращает исходное сообщение и его дубликат с другой меткой.
            /// </summary>
            [Test]
            public void should_pass_through_message()
            {
                var processor = new WireTap("boo.r1".ToMessageLabel());

                var message = new Message(
                    "boo".ToMessageLabel(),
                    new Dictionary<string, object> { { "This", "That" } },
                    "Body");

                var result = processor.Apply(message).ToList();

                result.Should().HaveCount(2, "Должно быть исходное сообщение и его дубликат.");
                result.Should().Contain(m => m.Label.ToString() == "boo.r1", "Должно быть сообщение c меткой boo.r1");
                result.Should().Contain(m => m.Label.ToString() == "boo", "Должно быть сообщение c меткой boo");
            }
        }
    }

    // ReSharper restore InconsistentNaming
}
