using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Contour.Caching;
using Contour.Flow.Configuration;
using Contour.Flow.Execution;
using Contour.Flow.Transport;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
// ReSharper disable InconsistentNaming

namespace Contour.Tests
{
    public class MessageFlowHandlingSpecs
    {
        [TestFixture]
        public class describe_flow
        {
            [Test]
            public void should_buffer_incoming_messages()
            {
                var factory = GetBroker();
                var flow = factory.Create<Payload>("fake");
                flow.On("incoming_label");

                flow.Entry().Post(new Payload());
            }

            [Test]
            public void should_perform_single_action_on_incoming_message()
            {
                var factory = GetBroker();
                var flow = factory.Create<Payload>("fake_transport");
                var action = A.Fake<Func<FlowContext<Payload>, Payload>>();

                flow.On("incoming_label")
                    .Act(action);

                var tcs = new TaskCompletionSource<bool>();
                A.CallTo(action).Invokes(() => tcs.SetResult(true));

                flow.Entry().Post(new Payload());

                tcs.Task.Wait(1.Minutes());
                A.CallTo(action).MustHaveHappened();
            }

            [Test]
            public void should_perform_single_action_with_no_result_on_incoming_message()
            {
                var factory = GetBroker();
                var flow = factory.Create<Payload>("fake");
                var action = A.Fake<Action<FlowContext<Payload>>>();

                flow.On("incoming_label")
                    .Act(action);

                var tcs = new TaskCompletionSource<bool>();
                A.CallTo(action).Invokes(() => tcs.SetResult(true));
                flow.Entry().Post(new Payload());

                tcs.Task.Wait(1.Minutes());
                A.CallTo(action).MustHaveHappened();
            }

            [Test]
            public void should_terminate_the_flow_on_action_errors()
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
            }

            [Test]
            public void should_cache_action_results()
            {
                var putTcs = new TaskCompletionSource<bool>();
                var getTcs = new TaskCompletionSource<bool>();

                var @in = new Payload() {Data = "123"};
                var @out = new NewPayload() {Value = 123};
                var period = TimeSpan.FromMinutes(5);

                var hashKeyProvider = new HashKeyProvider();
                var key = hashKeyProvider.Get(@in);

                var cacheProvider = A.Fake<ICacheProvider>();

                NewPayload cached = null;
                //fake a cache miss and a subsequent cache hit
                A.CallTo(() => cacheProvider.Get<NewPayload>(key)).Returns(null);
                    //or throws exception in some cache implementations
                A.CallTo(() => cacheProvider.Put(key, @out, period)).Invokes(() =>
                {
                    cached = @out;
                    putTcs.SetResult(true);
                });

                var policy = A.Fake<ICachePolicy>();
                A.CallTo(() => policy.CacheProvider).Returns(cacheProvider);
                A.CallTo(() => policy.KeyProvider).Returns(hashKeyProvider);
                A.CallTo(() => policy.Period).Returns(period);

                var factory = GetBroker();
                var flow = factory.Create<Payload>("fake");
                flow.On("incoming_label")
                    .Act(payload => @out, policy: policy);

                flow.Entry().Post(@in);
                putTcs.Task.Wait(1.Minutes());
                cached.Should().Be(@out);

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
                getTcs.Task.Wait(1.Minutes());
                fetched.Should().Be(@out);
            }
        }


        [Test]
        public void should_broadcast_action_results_by_type()
        {
            var tcsOne = new TaskCompletionSource<bool>();
            var tcsTwo = new TaskCompletionSource<bool>();

            var registry = A.Fake<IFlowRegistry>();
            var factory = A.Fake<IFlowFactory>();

            A.CallTo(() => factory.Create<Payload>(A.Dummy<string>()))
                .WithAnyArguments()
                .ReturnsLazily(() => new LocalMessageFlow<Payload>(new LocalFlowTransport()) { Registry = registry });

            A.CallTo(() => factory.Create<NewPayload>(A.Dummy<string>()))
                .WithAnyArguments()
                .ReturnsLazily(
                    () => new LocalMessageFlow<NewPayload>(new LocalFlowTransport()) { Registry = registry });

            var actionOne = A.Fake<Func<FlowContext<NewPayload>, NewPayload>>();
            A.CallTo(actionOne)
                .WithNonVoidReturnType()
                .Invokes(() => tcsOne.SetResult(true))
                .Returns(new NewPayload() { Value = 123 });

            var actionTwo = A.Fake<Func<FlowContext<NewPayload>, NewPayload>>();
            A.CallTo(actionTwo)
                .WithNonVoidReturnType()
                .Invokes(() => tcsTwo.SetResult(true))
                .Returns(new NewPayload() { Value = 345 });

            var receiverOneFlow = factory.Create<NewPayload>("receiver_1_transport");
            receiverOneFlow.On("receiver_label") //label is irrelevant when broadcasting
                .Act(actionOne);

            var receiverTwoFlow = factory.Create<NewPayload>("receiver_2_transport");
            receiverTwoFlow.On("receiver_label") //label is irrelevant when broadcasting
                .Act(actionTwo);

            A.CallTo(() => registry.GetAll()).Returns(new List<IFlowRegistryItem>() { receiverOneFlow, receiverTwoFlow });

            var senderFlow = factory.Create<Payload>("sender_transport");
            senderFlow.On("sender_label") //label is irrelevant when broadcasting
                .Broadcast(p => new NewPayload());

            senderFlow.Entry().Post(new Payload());

            Task.WaitAll(new Task[] { tcsOne.Task, tcsTwo.Task }, 1.Minutes());
            A.CallTo(actionOne).MustHaveHappened();
            A.CallTo(actionTwo).MustHaveHappened();
        }

        [Test]
        public void should_correlate_client_callbacks()
        {
            var tcsOne = new TaskCompletionSource<bool>();
            var tcsTwo = new TaskCompletionSource<bool>();

            var transport = new LocalFlowTransport();
            var broker = new FlowBroker();
            broker.Register("local", transport);

            var flow = broker.Create<Payload>("local")
                .On("label")
                .Act(p => new Payload() { Value = p.Value.Value * 2 })
                .Respond();

            var callbackOne = A.Fake<Action<FlowContext<Payload>>>();
            var callbackOneFake = A.CallTo(callbackOne)
                .Invokes(() => tcsOne.SetResult(true));

            var callbackTwo = A.Fake<Action<FlowContext<Payload>>>();
            var callbackTwoFake = A.CallTo(callbackTwo)
                .Invokes(() => tcsTwo.SetResult(true));

            var firstEntry = flow.Entry(callbackOne);
            var secondEntry = flow.Entry(callbackTwo);

            firstEntry.Post(new Payload() { Value = 1 });
            tcsOne.Task.Wait(1.Minutes());

            secondEntry.Post(new Payload() { Value = 2 });
            tcsTwo.Task.Wait(1.Minutes());

            callbackOneFake.MustHaveHappened(Repeated.Exactly.Once);
            callbackTwoFake.MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void should_execute_client_callback_on_response()
        {
            var tcs = new TaskCompletionSource<bool>();
            var transport = new LocalFlowTransport();
            var broker = new FlowBroker();
            broker.Register("local", transport);

            var flow = broker.Create<Payload>("local")
                .On("label")
                .Act(p => new Payload() { Value = p.Value.Value * 2 })
                .Respond();

            var result = 0.0;
            var entry = flow.Entry(ctx =>
            {
                result = ctx.Value.Value;
                tcs.SetResult(true);
            });
            entry.Post(new Payload() { Value = 1 });

            tcs.Task.Wait(1.Minutes());
            result.Should().Be(2);
        }

        private static IFlowBroker GetBroker()
        {
            var broker = A.Fake<IFlowBroker>();
            A.CallTo(() => broker.Create<Payload>(A.Dummy<string>()))
                .WithAnyArguments()
                .ReturnsLazily(() => new LocalMessageFlow<Payload>(new LocalFlowTransport()) {Registry = broker});
            return broker;
        }
    }
}