using System;
using Contour.Caching;
using Contour.Flow.Configuration;
using Contour.Flow.Transport;
using FakeItEasy;
using NUnit.Framework;
// ReSharper disable InconsistentNaming

namespace Contour.Tests
{
    public class MessageFlowConfigurationSpecs
    {
        [TestFixture]
        public class describe_message_flow_consumption_configuration
        {
            [Test]
            public void should_configure_incoming_flow()
            {
                var factory = GetBroker();
                factory.Create<Payload>("fake")
                    .On("incoming_label");
            }

            [Test]
            public void should_configure_incoming_flow_buffering()
            {
                const int capacity = 10;
                var factory = GetBroker();
                factory.Create<Payload>("fake")
                    .On("incoming_label", capacity);
            }
        }

        [TestFixture]
        public class describe_message_flow_actions_configuration
        {
            [Test]
            public void should_configure_incoming_flow_single_action()
            {
                var factory = GetBroker();
                factory.Create<Payload>("fake")
                    .On("incoming_label")
                    .Act(p => p);
            }

            [Test]
            public void should_configure_incoming_flow_single_action_with_no_result()
            {
                var factory = GetBroker();
                factory.Create<Payload>("fake")
                    .On("incoming_label")
                    .Act(arg => { });
            }

            [Test]
            public void should_configure_incoming_flow_multiple_actions()
            {
                var factory = GetBroker();
                factory.Create<Payload>("fake")
                    .On("incoming_label")
                    .Act(p => p)
                    .Act(p2 => p2);
            }

            [Test]
            public void should_configure_incoming_flow_multiple_type_actions()
            {
                var factory = GetBroker();
                factory.Create<Payload>("fake")
                    .On("incoming_label")
                    .Act(p => new NewPayload())
                    .Act(p2 => new Payload());
            }

            [Test]
            public void should_configure_incoming_flow_action_scalability()
            {
                const int scale = 10;

                var factory = GetBroker();
                factory.Create<Payload>("fake")
                    .On("incoming_label")
                    .Act(p => p, scale: scale);
            }

            [Test]
            public void should_configure_incoming_flow_action_buffering()
            {
                const int actionCapacity = 100;

                var factory = GetBroker();
                factory.Create<Payload>("fake")
                    .On("incoming_label")
                    .Act(p => p, capacity: actionCapacity);
            }
        }

        [TestFixture]
        public class describe_message_flow_propagation_configuration
        {
            [Test]
            public void should_configure_echo_response_on_incoming_flow()
            {
                var factory = GetBroker();

                factory.Create<Payload>("responder")
                    .On("incoming_label")
                    .Respond();
            }

            [Ignore]
            [Test]
            public void should_configure_direct_forward_of_incoming_flow()
            {
                var factory = GetBroker();
                factory.Create<Payload>("fake")
                    .On("incoming_label")
                    .Forward("outgoing_label");
            }

            [Test]
            public void should_configure_action_response()
            {
                var factory = GetBroker();

                factory.Create<Payload>("responder")
                    .On("incoming_label")
                    .Act(p => new NewPayload())
                    .Respond();
            }

            [Test]
            public void should_configure_action_response_with_callback()
            {
                var factory = GetBroker();
                var flow = factory.Create<Payload>("fake")
                    .On("incoming_label")
                    .Respond();

                flow.Entry(fc => { });
            }

            [Ignore]
            [Test]
            public void should_configure_action_result_forwarding()
            {
                var factory = GetBroker();
                factory.Create<Payload>("fake")
                    .On("incoming_label")
                    .Act(p => new NewPayload())
                    .Forward("outgoing_label");
            }

            [Test]
            public void should_configure_action_results_broadcasting_by_label()
            {
                var factory = GetBroker();
                factory.Create<Payload>("fake")
                    .On("incoming_label")
                    .Broadcast(p => new NewPayload(), "broadcast_label");
            }

            [Test]
            public void should_configure_action_results_broadcasting_by_type()
            {
                var factory = GetBroker();
                factory.Create<Payload>("fake")
                    .On("incoming_label")
                    .Broadcast(p => new NewPayload());
            }
        }

        [TestFixture]
        public class describe_message_flow_caching_configuration
        {
            [Test]
            public void should_configure_action_result_caching()
            {
                var cachePolicy = new DefaultCachePolicy(TimeSpan.FromSeconds(10));

                var factory = GetBroker();
                factory.Create<Payload>("fake")
                    .On("incoming_label")
                    .Act(p => new NewPayload(), policy: cachePolicy);
            }

            [Test]
            public void should_configure_action_result_caching_on_broadcast()
            {
                var cachePolicy = new DefaultCachePolicy(TimeSpan.FromSeconds(10));

                var factory = GetBroker();
                factory.Create<Payload>("fake")
                    .On("incoming_label")
                    .Broadcast(p => new NewPayload(), policy: cachePolicy);
            }

            [Test]
            public void should_configure_action_result_caching_and_responding()
            {
                var cachePolicy = new DefaultCachePolicy(TimeSpan.FromSeconds(10));

                var factory = GetBroker();

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

                var factory = GetBroker();
                factory.Create<Payload>("fake")
                    .On("incoming_label")
                    .Act(p => new NewPayload(), policy: cachePolicy)
                    .Forward("outgoing_label");
            }
        }

        private static IFlowBroker GetBroker()
        {
            var broker = A.Fake<IFlowBroker>();
            var flow = new LocalMessageFlow<Payload>(new LocalFlowTransport()) {Registry = broker};

            A.CallTo(() => broker.Create<Payload>(A.Dummy<string>()))
                .WithAnyArguments()
                .Returns(flow);

            A.CallTo(() => broker.Get(A.Dummy<string>()))
                .WithAnyArguments()
                .Returns(flow);

            return broker;
        }
    }
}