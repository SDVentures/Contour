using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using Common.Logging;

using Contour.Receiving;
using Contour.Testing.Transport.RabbitMq;
using Contour.Transport.RabbitMQ.Topology;

using FluentAssertions;

using NUnit.Framework;

namespace Contour.RabbitMq.Tests
{
    /// <summary>
    /// The basic request response specs.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
    public class MetricCollectingSpecs
    {
        /// <summary>
        /// The when_requesting_using_custom_callback_route.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class when_handling_request_message : RabbitMqFixture
        {
            #region Public Methods and Operators

            /// <summary>
            /// The should_return_response.
            /// </summary>
            [Test]
            public void should_collect_metrics_with_correct_tags()
            {
                IMessage message = null;

                IBus producer = this.StartBus(
                    "producer",
                    cfg =>
                        {
                            cfg.Route("dummy.request").
                               WithCallbackEndpoint(
                                   b =>
                                   {
                                       Exchange e = b.Topology.Declare(
                                           Exchange.Named("dummy.response").AutoDelete);
                                       Queue q = b.Topology.Declare(
                                           Queue.Named("dummy.response").AutoDelete.Exclusive);

                                       b.Topology.Bind(e, q);

                                       return new SubscriptionEndpoint(q, e);
                                   });
                            cfg.CollectMetrics(new TagsCheckingCollector("producer", "dummy.request"));
                        });

                this.StartBus(
                    "consumer",
                    cfg =>
                        {
                            cfg.On<DummyRequest>("dummy.request").
                               ReactWith(
                                   (m, ctx) =>
                                   {
                                       message = ctx.Message;
                                       ctx.Reply(new DummyResponse(m.Num * 2));
                                   });
                            cfg.CollectMetrics(new TagsCheckingCollector("consumer", "dummy.request"));
                        });

                Task response = producer.RequestAsync<DummyRequest, DummyResponse>("dummy.request", new DummyRequest(13));

                response.Wait(3.Seconds()).
                    Should().
                    BeTrue();
            }

            #endregion
        }


        private class TagsCheckingCollector : IMetricsCollector
        {
            private readonly string endpoint;
            
            private readonly string label;

            public TagsCheckingCollector(string endpoint, string label)
            {
                this.endpoint = endpoint;
                this.label = label;
            }

            public void Increment(string metricName, double sampleRate = 1, string[] tags = null)
            {
                this.EnsureTags(tags);
            }

            public void Decrement(string metricName, double sampleRate = 1, string[] tags = null)
            {
                this.EnsureTags(tags);
            }

            public void Histogram<T>(string metricName, T value, double sampleRate = 1, string[] tags = null)
            {
                this.EnsureTags(tags);
            }

            public void Gauge<T>(string metricName, T value, double sampleRate = 1, string[] tags = null)
            {
                this.EnsureTags(tags);
            }

            private void EnsureTags(string[] tags)
            {
                Assert.IsNotNull(tags, "tags should not be null");

                foreach (var tag in tags)
                {
                    AssertEquality(tag, nameof(this.endpoint), this.endpoint);
                    AssertEquality(tag, nameof(this.label), this.label);
                }
            }

            private static void AssertEquality(string value, string fieldName, string fieldValue)
            {
                LogManager.GetLogger<MetricCollectingSpecs>().Info(m => m($"value: '{value}', fieldName: '{fieldName}', fieldValue: '{fieldValue}'"));

                var prefix = fieldName + ":";
                if (value.StartsWith(prefix))
                {
                    if (string.IsNullOrEmpty(fieldValue))
                    {
                        LogManager.GetLogger<MetricCollectingSpecs>().Warn(m => m($"value: '{value}', fieldName: '{fieldName}', fieldValue: '{fieldValue}' EMPTY"));
                    }

                    Assert.IsNotEmpty(fieldValue, $"{fieldName} should not be null");
                    Assert.AreEqual(value, prefix + fieldValue, $"{value} should contain {fieldName}");
                }
            }
        }
    }
}