using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contour.Caching;
using Contour.Flow.Configuration;
using Contour.Flow.Execution;
using Contour.Flow.Transport;
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
                var flow = factory.Create<Payload>("fake");
                    flow.On("incoming_label");
                    
                flow.Entry().Post(new Payload());
            };

            it["should perform single action on incoming message"] = () =>
            {
                var factory = GetMessageFlowFactory();
                var flow = factory.Create<Payload>("fake");
                var action = A.Fake<Func<FlowContext<Payload>, Payload>>();

                flow.On("incoming_label")
                    .Act(action);

                var tcs = new TaskCompletionSource<bool>();
                A.CallTo(action).Invokes(() => tcs.SetResult(true));
                flow.Entry().Post(new Payload());

                tcs.Task.Wait(AMinute());
                A.CallTo(action).MustHaveHappened();
            };

            it["should perform single action with no result on incoming message"] = () =>
            {
                var factory = GetMessageFlowFactory();
                var flow = factory.Create<Payload>("fake");
                var action = A.Fake<Action<FlowContext<Payload>>>();

                flow.On("incoming_label")
                    .Act(action);

                var tcs = new TaskCompletionSource<bool>();
                A.CallTo(action).Invokes(() => tcs.SetResult(true));
                flow.Entry().Post(new Payload());

                tcs.Task.Wait(AMinute());
                A.CallTo(action).MustHaveHappened();
            };

            it["should terminate the flow on action errors"] = () =>
            {
                //var factory = GetMessageFlowFactory();
                //var flow = factory.Create<Payload>("fake");
                //var tcs = new TaskCompletionSource<bool>();

                //var errorAction = A.Fake<Func<Payload, NewPayload>>();
                //A.CallTo(errorAction).WithAnyArguments().Throws(new Exception());
                //A.CallTo(errorAction).Invokes(() => tcs.SetResult(true));

                //var nextAction = A.Fake<Func<FlowContext<Payload, NewPayload>, NewPayload>>();
                //A.CallTo(nextAction).DoesNothing();
                
                //flow.On("incoming_label")
                //    .Act(errorAction)
                //    .Act(nextAction);

                //flow.Entry.Post(new Payload());
                //tcs.Task.Wait(AMinute());

                //A.CallTo(errorAction).MustHaveHappened();
                //A.CallTo(nextAction).MustNotHaveHappened();
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
                var flow = factory.Create<Payload>("fake");
                    flow.On("incoming_label")
                    .Act(payload => @out)
                    .Cache<Payload, NewPayload>(policy);

                flow.Entry().Post(@in);
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

                flow.Entry().Post(@in);
                getTcs.Task.Wait(AMinute());
                fetched.should_be(@out);
            };

            it["should broadcast action results by type"] = () =>
            {
                var tcs = new TaskCompletionSource<bool>();

                var registry = A.Fake<IFlowRegistry>();
                var factory = A.Fake<IFlowFactory>();

                A.CallTo(() => factory.Create<Payload>(A.Dummy<string>()))
                    .WithAnyArguments()
                    .ReturnsLazily(() => new LocalMessageFlow<Payload>(new LocalFlowTransport()) {Registry = registry});

                A.CallTo(() => factory.Create<NewPayload>(A.Dummy<string>()))
                    .WithAnyArguments()
                    .ReturnsLazily(
                        () => new LocalMessageFlow<NewPayload>(new LocalFlowTransport()) {Registry = registry});

                var action = A.Fake<Func<FlowContext<NewPayload>, NewPayload>>();
                A.CallTo(action)
                    .WithReturnType<NewPayload>()
                    .Invokes(() => tcs.SetResult(true))
                    .Returns(new NewPayload() { Value = 123 });

                var receiverFlow = factory.Create<NewPayload>("receiver");
                receiverFlow.On("receiver_label") //label is irrelevant when broadcasting
                    .Act(action);

                A.CallTo(() => registry.GetAll<NewPayload>()).Returns(new List<IFlowRegistryItem>() { receiverFlow });

                var senderFlow = factory.Create<Payload>("sender");
                senderFlow.On("sender_label") //label is irrelevant when broadcasting
                    .Act(p => new NewPayload())
                    .Broadcast<Payload, NewPayload>();

                senderFlow.Entry().Post(new Payload());
                tcs.Task.Wait(AMinute());

                A.CallTo(action).MustHaveHappened();
            };
        }

        private static IFlowFactory GetMessageFlowFactory()
        {
            var factory = A.Fake<IFlowFactory>();
            A.CallTo(() => factory.Create<Payload>(A.Dummy<string>()))
                .WithAnyArguments()
                .ReturnsLazily(() => new LocalMessageFlow<Payload>(new LocalFlowTransport()));
            return factory;
        }

        private static TimeSpan AMinute()
        {
            return TimeSpan.FromMinutes(1);
        }
    }
}