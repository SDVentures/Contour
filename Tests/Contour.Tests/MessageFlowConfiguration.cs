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
    public class MessageFlowConfiguration
    {
        [TestFixture]
        public class describe_message_flow_consumption
        {
            [Test]
            public void should_configure_incoming_flow()
            {
                var factory = GetMessageFlowFactory();
                factory.Create<Payload>("fake")
                    .On("incoming_label");
            }

            [Test]
            public void should_configure_incoming_flow_buffering()
            {
                const int capacity = 10;
                var factory = GetMessageFlowFactory();
                factory.Create<Payload>("fake")
                    .On("incoming_label", capacity);
            }
        }

        [TestFixture]
        public class describe_message_flow_actions
        {
            [Test]
            public void should_configure_incoming_flow_single_action()
            {
                var factory = GetMessageFlowFactory();
                factory.Create<Payload>("fake")
                    .On("incoming_label")
                    .Act(p => p);
            }

            [Test]
            public void should_configure_incoming_flow_single_action_with_no_result()
            {
                var factory = GetMessageFlowFactory();
                factory.Create<Payload>("fake")
                    .On("incoming_label")
                    .Act(arg => { });
            }

            [Test]
            public void should_configure_incoming_flow_multiple_actions()
            {
                var factory = GetMessageFlowFactory();
                factory.Create<Payload>("fake")
                    .On("incoming_label")
                    .Act(p => p)
                    .Act(p2 => p2);
            }

            [Test]
            public void should_configure_incoming_flow_multiple_type_actions()
            {
                var factory = GetMessageFlowFactory();
                factory.Create<Payload>("fake")
                    .On("incoming_label")
                    .Act(p => new NewPayload())
                    .Act(p2 => new Payload());
            }

            [Test]
            public void should_configure_incoming_flow_action_scalability()
            {
                const int scale = 10;

                var factory = GetMessageFlowFactory();
                factory.Create<Payload>("fake")
                    .On("incoming_label")
                    .Act(p => p, scale: scale);
            }

            [Test]
            public void should_configure_incoming_flow_action_buffering()
            {
                const int actionCapacity = 100;

                var factory = GetMessageFlowFactory();
                factory.Create<Payload>("fake")
                    .On("incoming_label")
                    .Act(p => p, capacity: actionCapacity);
            }
        }

        [TestFixture]
        public class describe_message_flow_propagation
        {
            [Ignore]
            [Test]
            public void should_configure_echo_response_on_incoming_flow()
            {
                var factory = GetMessageFlowFactory();

                factory.Create<Payload>("responder")
                    .On("incoming_label")
                    .Respond();
            }

            [Ignore]
            [Test]
            public void should_configure_direct_forward_of_incoming_flow()
            {
                var factory = GetMessageFlowFactory();
                factory.Create<Payload>("fake")
                    .On("incoming_label")
                    .Forward("outgoing_label");
            }

            [Ignore]
            [Test]
            public void should_configure_action_response_on_incoming_flow()
            {
                var factory = GetMessageFlowFactory();

                factory.Create<Payload>("responder")
                    .On("incoming_label")
                    .Act(p => new NewPayload())
                    .Respond();
            }

            [Ignore]
            [Test]
            public void should_configure_action_result_forwarding()
            {
                var factory = GetMessageFlowFactory();
                factory.Create<Payload>("fake")
                    .On("incoming_label")
                    .Act(p => new NewPayload())
                    .Forward("outgoing_label");
            }

            [Test]
            public void should_broadcast_action_results_by_label()
            {
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

                Task.WaitAll(new Task[] {tcsOne.Task, tcsTwo.Task}, 1.Minutes());
                A.CallTo(actionOne).MustHaveHappened();
                A.CallTo(actionTwo).MustHaveHappened();
            }

            [Ignore]
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
                    .Act(p => new Payload() { Value = p.In.Value * 2 })
                    .Respond();

                var callbackOne = A.Fake<Action<FlowContext<FlowContext<Payload>, Payload>>>();
                var callbackOneFake = A.CallTo(callbackOne)
                    .WithNonVoidReturnType()
                    .Invokes(() => tcsOne.SetResult(true));

                var callbackTwo = A.Fake<Action<FlowContext<FlowContext<Payload>, Payload>>>();
                var callbackTwoFake = A.CallTo(callbackTwo)
                    .WithNonVoidReturnType()
                    .Invokes(() => tcsTwo.SetResult(true));

                var firstEntry = flow.Entry(callbackOne);
                var secondEntry = flow.Entry(callbackTwo);

                firstEntry.Post(new Payload() { Value = 1 });
                tcsOne.Task.Wait(1.Minutes());

                callbackOneFake.MustHaveHappened();
                callbackTwoFake.MustNotHaveHappened();

                secondEntry.Post(new Payload() { Value = 2 });
                tcsTwo.Task.Wait(1.Minutes());

                callbackTwoFake.MustHaveHappened();
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
                    .Act(p => new Payload() { Value = p.In.Value * 2 })
                    .Respond();

                var result = 0.0;
                var entry = flow.Entry(ctx =>
                {
                    result = ctx.Out.Value;
                    tcs.SetResult(true);
                });
                entry.Post(new Payload() { Value = 1 });

                tcs.Task.Wait(1.Minutes());
                result.Should().Be(2);
            }
        }

        [TestFixture]
        public class describe_message_flow_caching
        {
            [Test]
            public void should_configure_action_result_caching()
            {
                var cachePolicy = new DefaultCachePolicy(TimeSpan.FromSeconds(10));

                var factory = GetMessageFlowFactory();
                factory.Create<Payload>("fake")
                    .On("incoming_label")
                    .Act(p => new NewPayload(), policy: cachePolicy);
            }

            [Ignore]
            [Test]
            public void should_configure_action_result_caching_and_responding()
            {
                var cachePolicy = new DefaultCachePolicy(TimeSpan.FromSeconds(10));

                var factory = GetMessageFlowFactory();

                factory.Create<Payload>("responder")
                    .On("incoming_label")
                    .Act(p => new NewPayload(), policy: cachePolicy)
                    .Respond();
            }

            [Ignore]
            [Test]
            public void should_configure_action_result_caching_and_forwarding()
            {
                var cachePolicy = new DefaultCachePolicy(TimeSpan.FromSeconds(10));

                var factory = GetMessageFlowFactory();
                factory.Create<Payload>("fake")
                    .On("incoming_label")
                    .Act(p => new NewPayload(), policy: cachePolicy)
                    .Forward("outgoing_label");
            }
        }

        private static IFlowFactory GetMessageFlowFactory()
        {
            var factory = A.Fake<IFlowFactory>();
            A.CallTo(() => factory.Create<Payload>(A.Dummy<string>()))
                .WithAnyArguments()
                .Returns(new LocalMessageFlow<Payload>(new LocalFlowTransport()));
            return factory;
        }
    }
}