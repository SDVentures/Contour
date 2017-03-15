using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contour.Filters;
using Contour.Helpers;
using Contour.Sending;
using Moq;
using NUnit.Framework;

namespace Contour.Common.Tests
{
    public class AbstactSenderSpecs
    {
        [TestFixture]
        public class WhenUsingAbstactSender
        {
            [Test]
            public void ShouldReplaceCorrelationIdOnRequestWithLabel()
            {
                var storageMock = new Mock<IIncomingMessageHeaderStorage>();
                var incomingHeaders = new Dictionary<string, object>
                {
                    [Headers.OriginalMessageId] = "OriginalMessageId",
                    [Headers.CorrelationId] = "CorrelationId"
                };
                storageMock.Setup(s => s.Load())
                    .Returns(
                        incomingHeaders);
                var senderConfigMock = new Mock<ISenderConfiguration>();
                senderConfigMock.Setup(s => s.Options)
                    .Returns(
                        new SenderOptions
                            {
                                IncomingMessageHeaderStorage = new Maybe<IIncomingMessageHeaderStorage>(storageMock.Object)
                            });
                senderConfigMock.Setup(s => s.Label)
                    .Returns(MessageLabel.Any);

                IMessage sentMessage = null;
                var sut = new TestSender(
                    me =>
                        {
                            sentMessage = me.Out;
                            me.In = new Message(MessageLabel.Any, new object());
                            TaskCompletionSource<MessageExchange> tcs = new TaskCompletionSource<MessageExchange>();
                            tcs.SetResult(me);
                            return tcs.Task;
                        },
                    new Mock<IEndpoint>().Object,
                    senderConfigMock.Object,
                    Enumerable.Empty<IMessageExchangeFilter>());

                var r = sut.Request<object>(MessageLabel.Empty, new { }, new RequestOptions()).Result;

                Assert.IsNotNull(sentMessage);
                Assert.AreEqual(incomingHeaders[Headers.OriginalMessageId], sentMessage.Headers[Headers.OriginalMessageId]);
                Assert.AreNotEqual(incomingHeaders[Headers.CorrelationId], sentMessage.Headers[Headers.CorrelationId]);
            }

            [Test]
            public void ShouldReplaceCorrelationIdOnRequestWithoutLabel()
            {
                var storageMock = new Mock<IIncomingMessageHeaderStorage>();
                var incomingHeaders = new Dictionary<string, object>
                {
                    [Headers.OriginalMessageId] = "OriginalMessageId",
                    [Headers.CorrelationId] = "CorrelationId"
                };
                storageMock.Setup(s => s.Load())
                    .Returns(
                        incomingHeaders);
                var senderConfigMock = new Mock<ISenderConfiguration>();
                senderConfigMock.Setup(s => s.Options)
                    .Returns(
                        new SenderOptions
                        {
                            IncomingMessageHeaderStorage = new Maybe<IIncomingMessageHeaderStorage>(storageMock.Object)
                        });
                senderConfigMock.Setup(s => s.Label)
                    .Returns(MessageLabel.Any);

                IMessage sentMessage = null;
                var sut = new TestSender(
                    me =>
                    {
                        sentMessage = me.Out;
                        me.In = new Message(MessageLabel.Any, new object());
                        TaskCompletionSource<MessageExchange> tcs = new TaskCompletionSource<MessageExchange>();
                        tcs.SetResult(me);
                        return tcs.Task;
                    },
                    new Mock<IEndpoint>().Object,
                    senderConfigMock.Object,
                    Enumerable.Empty<IMessageExchangeFilter>());

                var r = sut.Request<object>(new { }, new RequestOptions()).Result;

                Assert.IsNotNull(sentMessage);
                Assert.AreEqual(incomingHeaders[Headers.OriginalMessageId], sentMessage.Headers[Headers.OriginalMessageId]);
                Assert.AreNotEqual(incomingHeaders[Headers.CorrelationId], sentMessage.Headers[Headers.CorrelationId]);
            }
        }

        private class TestSender : AbstractSender
        {
            private readonly Func<MessageExchange, Task<MessageExchange>> internalSend;

            public TestSender(Func<MessageExchange, Task<MessageExchange>> internalSend, IEndpoint endpoint, ISenderConfiguration configuration, IEnumerable<IMessageExchangeFilter> filters)
                : base(endpoint, configuration, filters)
            {
                this.internalSend = internalSend;
            }

            public override bool IsHealthy { get; } = true;

            public override void Start()
            {
            }

            public override void Stop()
            {
            }

            public override void Dispose()
            {
            }

            protected override Task<MessageExchange> InternalSend(MessageExchange exchange)
            {
                return this.internalSend(exchange);
            }
        }
    }
}
