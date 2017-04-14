﻿using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Contour.Transport.RabbitMq.Internal;
using FluentAssertions;
using Moq;
using NUnit.Framework;
// ReSharper disable InconsistentNaming

namespace Contour.RabbitMq.Tests
{
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here."), SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    public class RoundRobinSelectorSpecs
    {
        [TestFixture]
        public class when_declaring_round_robin_selector
        {
            [Test]
            public void should_use_not_null_collection()
            {
                Assert.Throws<ArgumentNullException>(() => new RoundRobinSelector(null));
            }

            [Test]
            public void should_use_not_empty_collection()
            {
                Assert.Throws<ArgumentOutOfRangeException>(
                    () => new RoundRobinSelector(new ConcurrentQueue<IProducer>(Enumerable.Empty<IProducer>())));
            }

            [Test]
            public void should_return_producers_sequentially_in_a_loop()
            {
                const int Count = 3;
                const int LoopFactor = 5;
                const int Size = Count * LoopFactor;

                var producers = Enumerable.Range(0, Count).Select(i => new Mock<IProducer>().Object);
                var list = producers.ToList();

                var selector = new RoundRobinSelector(new ConcurrentQueue<IProducer>(list));

                for (var i = 0; i < Size; i++)
                {
                    var producer = selector.Next();
                    var index = list.IndexOf(producer);

                    index.Should().Be(i % Count);
                }
            }

            [Test]
            public void should_update_sequence_on_full_round_if_producer_is_added()
            {
                const int Count = 3;
                const int LoopFactor = 5;

                var producers = Enumerable.Range(0, Count).Select(i => new Mock<IProducer>().Object);

                var queue = new ConcurrentQueue<IProducer>(producers);
                var selector = new RoundRobinSelector(queue);
                
                for (var i = 0; i < Count; i++)
                {
                    selector.Next();
                    
                    if (i == Count - 1)
                    {
                        queue.Enqueue(new Mock<IProducer>().Object);
                    }
                }

                var size = (Count + 1) * LoopFactor;
                var list = queue.ToList();
                for (int i = 0; i < size; i++)
                {
                    var producer = selector.Next();
                    var index = list.IndexOf(producer);

                    index.Should().Be(i % (Count + 1));
                }
            }
        }
    }
}