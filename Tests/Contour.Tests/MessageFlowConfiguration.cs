using System;
using System.Collections.Generic;
using System.Diagnostics;
using Contour.Caching;
using Contour.Flow.Configuration;
using FakeItEasy;

namespace Contour.Tests
{
    public class MessageFlowConfiguration
    {
        public class MessageFlowConsumption : NSpec
        {
            public void describe_message_consumption()
            {
                it["should configure incoming flow"] = () =>
                {
                    var factory = GetMessageFlowFactory();
                    factory.Create("fake")
                        .On<Payload>("incoming_label");
                };

                it["should configure incoming flow buffering"] = () =>
                {
                    const int capacity = 10;
                    var factory = GetMessageFlowFactory();
                    factory.Create("fake")
                        .On<Payload>("incoming_label", capacity);
                };
            }
        }

        public class MessageFlowActions : NSpec
        {
            public void describe_message_flow_actions()
            {
                it["should configure incoming flow single action"] = () =>
                {
                    var factory = GetMessageFlowFactory();
                    factory.Create("fake")
                        .On<Payload>("incoming_label")
                        .Act(p => p);
                };

                it["should configure incoming flow multiple actions"] = () =>
                {
                    var factory = GetMessageFlowFactory();
                    factory.Create("fake")
                        .On<Payload>("incoming_label")
                        .Act(p => p)
                        .Act(p2 => p2);
                };

                it["should configure incoming flow multiple type actions"] = () =>
                {
                    var factory = GetMessageFlowFactory();
                    factory.Create("fake")
                        .On<Payload>("incoming_label")
                        .Act(p => new NewPayload())
                        .Act(p2 => new Payload());
                };


                it["should configure incoming flow action scalability"] = () =>
                {
                    const int scale = 10;

                    var factory = GetMessageFlowFactory();
                    factory.Create("fake")
                        .On<Payload>("incoming_label")
                        .Act(p => p, scale);
                };

                it["should configure incoming flow action buffering"] = () =>
                {
                    const int actionCapacity = 100;

                    var factory = GetMessageFlowFactory();
                    factory.Create("fake")
                        .On<Payload>("incoming_label")
                        .Act(p => p, actionCapacity);
                };
            }
        }

        public class MessageFlowPropagation : NSpec
        {
            public void describe_message_flow_propagation()
            {
                xit["should configure echo response on incoming flow"] = () =>
                {
                    var factory = GetMessageFlowFactory();
                    factory.Create("fake")
                        .On<Payload>("incoming_label")
                        .Respond();
                };

                xit["should configure direct forward of incoming flow"] = () =>
                {
                    var factory = GetMessageFlowFactory();
                    factory.Create("fake")
                        .On<Payload>("incoming_label")
                        .Forward("outgoing_label");
                };

                xit["should configure action response on incoming flow"] = () =>
                {
                    var factory = GetMessageFlowFactory();
                    factory.Create("fake")
                        .On<Payload>("incoming_label")
                        .Act(p => new NewPayload())
                        .Respond();
                };

                xit["should configure action result forwarding"] = () =>
                {
                    var factory = GetMessageFlowFactory();
                    factory.Create("fake")
                        .On<Payload>("incoming_label")
                        .Act(p => new NewPayload())
                        .Forward("outgoing_label");
                };

                it["should configure action result broadcasting by type"] = () =>
                {
                    var factory = A.Fake<IFlowFactory>();
                    var registry = A.Fake<IFlowRegistry>();

                    A.CallTo(() => factory.Create(A.Dummy<string>()))
                        .WithAnyArguments()
                        .ReturnsLazily(() => new InMemoryMessageFlow() {Registry = registry}); //lazily means 'new value from factory function'

                    var flow1 = factory.Create("flow1");
                    flow1.On<NewPayload>("fake_label") //todo need no label here, provide an overloaded method for in-memory flows
                        .Act(p => p);

                    A.CallTo(() => registry.Get<NewPayload>()).Returns(new List<IFlowTarget>() {flow1});

                    factory.Create("flow0")
                        .On<Payload>("incoming_label")
                        .Act(p => new NewPayload())
                        .Broadcast<Payload, NewPayload>();
                };
            }
        }

        public class MessageFlowCaching : NSpec
        {
            public void describe_message_flow_caching()
            {
                it["should configure action result caching"] = () =>
                {
                    var cachePolicy = new DefaultCachePolicy(TimeSpan.FromSeconds(10));

                    var factory = GetMessageFlowFactory();
                    factory.Create("fake")
                        .On<Payload>("incoming_label")
                        .Act(p => new NewPayload())
                        .Cache<Payload, NewPayload>(cachePolicy);
                };

                xit["should configure action result caching and responding"] = () =>
                {
                    var cachePolicy = new DefaultCachePolicy(TimeSpan.FromSeconds(10));

                    var factory = GetMessageFlowFactory();
                    factory.Create("fake")
                        .On<Payload>("incoming_label")
                        .Act(p => new NewPayload())
                        .Cache<Payload, NewPayload>(cachePolicy)
                        .Respond();
                };

                xit["should configure action result caching and forwarding"] = () =>
                {
                    var cachePolicy = new DefaultCachePolicy(TimeSpan.FromSeconds(10));

                    var factory = GetMessageFlowFactory();
                    factory.Create("fake")
                        .On<Payload>("incoming_label")
                        .Act(p => new NewPayload())
                        .Cache<Payload, NewPayload>(cachePolicy)
                        .Forward("outgoing_label");
                };
            }
        }

        private static IFlowFactory GetMessageFlowFactory()
        {
            var factory = A.Fake<IFlowFactory>();
            A.CallTo(() => factory.Create("fake")).Returns(new InMemoryMessageFlow());
            return factory;
        }
    }
}
