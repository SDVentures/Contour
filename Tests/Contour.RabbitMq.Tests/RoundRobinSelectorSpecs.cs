using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Contour.Transport.RabbitMQ.Internal;
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
                    () => new RoundRobinSelector(Enumerable.Empty<Producer>().ToList()));
            }

            [Test]
            public void should_return_producers_sequentially_in_a_loop()
            {
                const int Count = 3;
                const int LoopFactor = 5;
                const int Size = Count * LoopFactor;

                var producers = Enumerable.Range(0, Count).Select(i => new Mock<Producer>()).ToList();
                var selector = new RoundRobinSelector(producers);

                for (var i = 0; i < Size; i++)
                {
                    var producer = selector.Next<Mock<Producer>>();
                    var index = producers.IndexOf(producer);

                    index.Should().Be(i % Count);
                }
            }
        }
    }
}