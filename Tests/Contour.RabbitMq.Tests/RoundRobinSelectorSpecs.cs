using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        public class given_a_round_robin_selector
        {
            [Test]
            public void should_use_not_null_collection()
            {
                Assert.Throws<ArgumentNullException>(() => new RoundRobinSelector(null));
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

            [Test]
            public void should_raise_error_if_all_producers_have_been_removed()
            {
                const int Count = 3;

                var producers = Enumerable.Range(0, Count).Select(i => new Mock<IProducer>().Object);

                var queue = new ConcurrentQueue<IProducer>(producers);
                var selector = new RoundRobinSelector(queue);

                IProducer p;
                while (queue.TryDequeue(out p))
                {
                }

                for (var i = 0; i < Count; i++)
                {
                    selector.Next();
                }
                
                Assert.Throws<Exception>(() => { selector.Next(); });
            }

            [Test]
            public void should_be_thread_safe()
            {
                const int ProducerCount = 1;
                const int MaxDelay = 5;
                const int ThreadCount = 200;
                var tcs = new TaskCompletionSource<bool>();
                var result = true;
                
                var producers = Enumerable.Range(0, ProducerCount).Select(i => new Mock<IProducer>().Object);
                var queue = new ConcurrentQueueWithDelays<IProducer>(producers, MaxDelay);
                var selector = new RoundRobinSelector(queue);

                var accessors = Enumerable.Range(0, ThreadCount).Select(i => new Thread(() =>
                {
                    while (true)
                    {
                        try
                        {
                            var producer = selector.Next();
                            producer.Should().NotBeNull();

                            Debug.WriteLine("Accessor succeeded");
                        }
                        catch(Exception ex)
                        {
                            result = false;
                            tcs.SetResult(false);
                            Debug.WriteLine($"Accessor failed: {ex.Message}");
                        }
                    }
                }));

                accessors.All(t =>
                {
                    t.Start();
                    return true;
                }).Should().BeTrue();

                var task = tcs.Task;
                task.Wait(1.Minutes()).Should().BeFalse();

                result.Should().BeTrue();
            }
        }

        private class ConcurrentQueueWithDelays<T> : ConcurrentQueue<T>, IEnumerable<T>
        {
            private readonly Random delayStream;
            private readonly int maxDelay;

            public ConcurrentQueueWithDelays(IEnumerable<T> collection, int maxDelay) : base(collection)
            {
                this.maxDelay = maxDelay;
                this.delayStream = new Random();
            }
            
            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                Thread.Sleep(TimeSpan.FromSeconds(this.delayStream.Next(this.maxDelay)));
                return base.GetEnumerator();
            }
        }
    }
}