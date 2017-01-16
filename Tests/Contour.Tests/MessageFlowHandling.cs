using System;
using System.Threading.Tasks;
using Contour.Caching;
using Contour.Flow.Configuration;
using FakeItEasy;
using NSpec;

namespace Contour.Tests
{
    public class MessageFlowHandling : NSpec
    {
        public void describe_flow()
        {
            it["should buffer incoming messages"] = () =>
            {
                var factory = GetMessageFlowFactory();
                var flow = factory.Create("fake");
                
                flow.On<Payload>("incoming_label");
                flow.Post(new Payload());
            };

            it["should perform single action on incoming message"] = () =>
            {
                var factory = GetMessageFlowFactory();
                var flow = factory.Create("fake");
                var action = A.Fake<Func<Payload, Payload>>();

                factory.Create("fake")
                    .On<Payload>("incoming_label")
                    .Act(action);

                var tcs = new TaskCompletionSource<bool>();
                A.CallTo(action).Invokes(() => tcs.SetResult(true));
                flow.Post(new Payload());

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

                var factory = GetMessageFlowFactory();
                var flow = factory.Create("fake");
                factory.Create("fake")
                    .On<Payload>("incoming_label")
                    .Act(payload => @out)
                    .Cache<Payload, NewPayload>(policy);

                flow.Post(@in);
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

                flow.Post(@in);
                getTcs.Task.Wait(AMinute());
                fetched.should_be(@out);
            };
        }

        private static IFlowFactory GetMessageFlowFactory()
        {
            var factory = A.Fake<IFlowFactory>();
            A.CallTo(() => factory.Create("fake")).Returns(new InMemoryMessageFlow());
            return factory;
        }

        private static TimeSpan AMinute()
        {
            return TimeSpan.FromMinutes(1);
        }
    }
}