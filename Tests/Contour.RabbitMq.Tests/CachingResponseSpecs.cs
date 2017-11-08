using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using Contour.Caching;
using Contour.Filters;
using Contour.Helpers;
using Contour.Serialization;
using Contour.Testing.Transport.RabbitMq;

using NUnit.Framework;

namespace Contour.RabbitMq.Tests
{
    // ReSharper disable InconsistentNaming

    /// <summary>
    /// Проверяет работу кеша.
    /// </summary>
    public class CachingResponseSpecs
    {
        /// <summary>
        /// В случае отправки нескольких одинаковых запросов
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class WhenSendingMultipleEqualRequestsToCachingAwareConsumer : RabbitMqFixture
        {
            /// <summary>
            /// Должно отослаться одно сообщение.
            /// </summary>
            [Test]
            public void ShouldSendOneMessageThroughTransport()
            {
                int result = 0;
                int consumerCalled = 0;

                IBus producer = this.StartBus(
                    "producer",
                    cfg =>
                        {
                            cfg.Route("dummy.request").WithDefaultCallbackEndpoint();
                            var cachingConfig = new Dictionary<string, CacheConfiguration>();
                            cachingConfig["dummy.request"] = new CacheConfiguration(true, 5.Seconds(), new MemoryCache(new Sha256Hasher(new JsonNetPayloadConverter())));
                            var cachingFilterDecorator = new CachingFilterDecorator(cachingConfig);
                            cfg.RegisterDecoratorOf<SendingExchangeFilter>(cachingFilterDecorator);
                        });
                this.StartBus(
                    "consumer",
                    cfg => cfg.On<DummyRequest>("dummy.request")
                        .ReactWith(
                                   (m, ctx) =>
                                       {
                                           consumerCalled++;
                                           ctx.Reply(new DummyResponse(m.Num * 2), Expires.In(5.Seconds()));
                                       }));

                Enumerable.Range(1, 3)
                    .ForEach(
                        _ =>
                            {
                                Task<int> response = producer
                                    .RequestAsync<DummyRequest, DummyResponse>("dummy.request", new DummyRequest(13))
                                    .ContinueWith(m => result = m.Result.Num);
                                response.Wait(3.Seconds()).Should().BeTrue();
                                result.Should().Be(26);
                            });

                consumerCalled.Should().Be(1);
            }
        }
    }
    // ReSharper restore InconsistentNaming
}
