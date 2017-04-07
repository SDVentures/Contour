﻿using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Contour.Transport.RabbitMQ;
using Contour.Transport.RabbitMQ.Internal;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Contour.RabbitMq.Tests
{
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here."),
     SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here."),
     Category("Unit")]
    public class FaultTolerantProducerSpecs
    {
        [Test]
        public void should_iterate_producers_on_send_failures()
        {
            const int Count = 5;

            var producers = Enumerable.Range(0, Count).Select(i =>
            {
                var mock = new Mock<IProducer>();
                mock
                    .Setup(p => p.Publish(It.IsAny<IMessage>()))
                    .Throws(new Exception("Publish error"));
                mock
                    .Setup(p => p.BrokerUrl).Returns(() => $"fake.url.{DateTime.Now.Ticks}");

                return mock.Object;
            });

            var selector = new RoundRobinSelector(new ConcurrentQueue<IProducer>(producers));
            var producer = new FaultTolerantProducer(selector, Count);

            var message = new Message<DummyRequest>(MessageLabel.Any, new DummyRequest(1));
            var exchange = new MessageExchange(message);

            try
            {
                producer.Try(exchange);
                Assert.Fail();
            }
            catch (FailoverException fex)
            {
                fex.Attempts.Should().Be(Count);
            }
        }

        [Test]
        public void should_aggregate_attempt_errors()
        {
            const int Count = 5;

            var producers = Enumerable.Range(0, Count).Select(i =>
            {
                var mock = new Mock<IProducer>();
                mock
                    .Setup(p => p.Publish(It.IsAny<IMessage>()))
                    .Throws(new Exception("Publish error"));
                mock
                    .Setup(p => p.BrokerUrl).Returns(() => $"fake.url.{DateTime.Now.Ticks}");

                return mock.Object;
            });

            var selector = new RoundRobinSelector(new ConcurrentQueue<IProducer>(producers));
            var producer = new FaultTolerantProducer(selector, Count);

            var message = new Message<DummyRequest>(MessageLabel.Any, new DummyRequest(1));
            var exchange = new MessageExchange(message);

            try
            {
                producer.Try(exchange);
                Assert.Fail();
            }
            catch (FailoverException fex)
            {
                fex.InnerException.Should().BeOfType<AggregateException>();
                var errors = (AggregateException)fex.InnerException;
                errors.InnerExceptions.Count.Should().Be(Count);
            }
        }
    }
}
