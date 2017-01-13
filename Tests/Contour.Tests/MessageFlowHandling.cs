using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Contour.Caching;
using Contour.Flow.Configuration;
using FakeItEasy;
using FluentAssertions;
using NSpec;

namespace Contour.Tests
{
    public class MessageFlowHandling : NSpec
    {
        public void describe_flow()
        {
            it["should buffer incoming messages"] = () =>
            {
                BufferBlock<Payload> buffer;
                var factory = GetMessageFlowFactory(out buffer);

                factory.Build().On<Payload>("incoming_label");
                buffer.Post(new Payload());
            };

            it["should perform single action on incoming message"] = () =>
            {
                BufferBlock<Payload> buffer;
                var factory = GetMessageFlowFactory(out buffer);

                var action = A.Fake<Func<Payload, Payload>>();
                factory.Build()
                    .On<Payload>("incoming_label")
                    .Act(action);

                var tcs = new TaskCompletionSource<bool>();
                A.CallTo(action).Invokes(() => tcs.SetResult(true));
                buffer.Post(new Payload());

                tcs.Task.Wait(AMinute());
                A.CallTo(action).MustHaveHappened();
            };
        }

        public void describe_flow_management()
        {
            it["should cache action results"] = () =>
            {
                var putTcs = new TaskCompletionSource<bool>();
                var getTcs = new TaskCompletionSource<bool>();

                var @in = new Payload() { Data = "123" };
                var @out = new NewPayload() { Value = 123 };
                var period = TimeSpan.FromMinutes(5);

                var hashKeyProvider = new HashKeyProvider();
                var key = hashKeyProvider.Get(@in);

                var cacheProvider = A.Fake<ICacheProvider>();

                NewPayload cached = null;
                //fake a cache miss and a subsequent cache hit
                A.CallTo(() => cacheProvider.Get<NewPayload>(key)).Returns(null); //or throws exception in some cache implementations
                A.CallTo(() => cacheProvider.Put(key, @out, period)).Invokes(() =>
                {
                    cached = @out;
                    putTcs.SetResult(true);
                });

                var policy = A.Fake<ICachePolicy>();
                A.CallTo(() => policy.CacheProvider).Returns(cacheProvider);
                A.CallTo(() => policy.KeyProvider).Returns(hashKeyProvider);
                A.CallTo(() => policy.Period).Returns(period);

                BufferBlock<Payload> buffer;
                var factory = GetMessageFlowFactory(out buffer);
                factory.Build()
                    .On<Payload>("incoming_label")
                    .Act(payload => @out)
                    .Cache<Payload, NewPayload>(policy);

                buffer.Post(@in);
                putTcs.Task.Wait(AMinute());
                cached.should_be(@out);

                //check cache hit
                NewPayload fetched = null;
                A.CallTo(() => cacheProvider.Get<NewPayload>(key))
                    .Invokes(() =>
                    {
                        fetched = @out;
                        getTcs.SetResult(true);
                    })
                    .Returns(@out);

                buffer.Post(@in);
                getTcs.Task.Wait(AMinute());
                fetched.should_be(@out);
            };
        }

        private static IMessageFlowFactory GetMessageFlowFactory(out BufferBlock<Payload> buffer)
        {
            var factory = A.Fake<IMessageFlowFactory>();
            buffer = new BufferBlock<Payload>();
            A.CallTo(() => factory.Build()).Returns(new MessageFlow(buffer));
            return factory;
        }

        private static TimeSpan AMinute()
        {
            return TimeSpan.FromMinutes(1);
        }
    }
}