using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Contour.Caching;
using Contour.Flow.Configuration;
using Contour.Flow.Execution;
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

                flow.On<Payload>("incoming_label")
                    .Act(action);

                var tcs = new TaskCompletionSource<bool>();
                A.CallTo(action).Invokes(() => tcs.SetResult(true));
                flow.Post(new Payload());

                tcs.Task.Wait(AMinute());
                A.CallTo(action).MustHaveHappened();
            };

            it["should terminate the flow on action errors"] = () =>
            {
                var factory = GetMessageFlowFactory();
                var flow = factory.Create("fake");
                var tcs = new TaskCompletionSource<bool>();

                var errorAction = A.Fake<Func<Payload, NewPayload>>();
                A.CallTo(errorAction).WithAnyArguments().Throws(new Exception());
                A.CallTo(errorAction).Invokes(() => tcs.SetResult(true));

                var nextAction = A.Fake<Func<ActionContext<Payload, NewPayload>, NewPayload>>();
                A.CallTo(nextAction).DoesNothing();
                
                flow.On<Payload>("incoming_label")
                    .Act(errorAction)
                    .Act(nextAction);

                flow.Post(new Payload());
                tcs.Task.Wait(AMinute());

                A.CallTo(errorAction).MustHaveHappened();
                A.CallTo(nextAction).MustNotHaveHappened();
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
                    flow.On<Payload>("incoming_label")
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

            it["should broadcast action results by type"] = () =>
            {
                var tcs = new TaskCompletionSource<bool>();

                var registry = A.Fake<IFlowRegistry>();
                var factory = A.Fake<IFlowFactory>();
                A.CallTo(() => factory.Create(A.Dummy<string>()))
                    .WithAnyArguments()
                    .ReturnsLazily(() => new InMemoryMessageFlow() {Registry = registry});

                var action = A.Fake<Func<Payload, NewPayload>>();
                A.CallTo(action)
                    .WithReturnType<NewPayload>()
                    .Invokes(() => tcs.SetResult(true))
                    .Returns(new NewPayload() { Value = 123 });

                var receiverFlow = factory.Create("receiver");
                receiverFlow.On<NewPayload>("receiver_label") //label is irrelevant when broadcasting
                    .Act(action);

                A.CallTo(() => registry.Get<NewPayload>()).Returns(new List<IFlowTarget>() { receiverFlow });

                var senderFlow = factory.Create("sender");
                senderFlow.On<Payload>("sender_label") //label is irrelevant when broadcasting
                    .Act(p => new NewPayload())
                    .Broadcast<Payload, NewPayload>();

                senderFlow.Post(new Payload());
                tcs.Task.Wait(AMinute());

                A.CallTo(action).MustHaveHappened();
            };
        }

        private static IFlowFactory GetMessageFlowFactory()
        {
            var factory = A.Fake<IFlowFactory>();
            A.CallTo(() => factory.Create(A.Dummy<string>()))
                .WithAnyArguments()
                .ReturnsLazily(() => new InMemoryMessageFlow());
            return factory;
        }

        private static TimeSpan AMinute()
        {
            return TimeSpan.FromMinutes(1);
        }
    }
}