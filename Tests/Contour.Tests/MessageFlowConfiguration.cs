using System;
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
                    MessageFlowFactory.Build()
                        .On<Payload>("incoming_label");
                };

                it["should configure incoming flow buffering"] = () =>
                {
                    const int capacity = 10;
                    MessageFlowFactory.Build()
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
                    MessageFlowFactory.Build()
                        .On<Payload>("incoming_label")
                        .Act(p => p);
                };

                it["should configure incoming flow multiple actions"] = () =>
                {
                    MessageFlowFactory.Build()
                        .On<Payload>("incoming_label")
                        .Act(p => p)
                        .Act(p2 => p2);
                };

                it["should configure incoming flow multiple type actions"] = () =>
                {
                    MessageFlowFactory.Build()
                        .On<Payload>("incoming_label")
                        .Act(p => new NewPayload())
                        .Act(p2 => new Payload());
                };


                it["should configure incoming flow action scalability"] = () =>
                {
                    const int scale = 10;

                    MessageFlowFactory.Build()
                        .On<Payload>("incoming_label")
                        .Act(p => p, scale);
                };

                it["should configure incoming flow action buffering"] = () =>
                {
                    const int actionCapacity = 100;

                    MessageFlowFactory.Build()
                        .On<Payload>("incoming_label")
                        .Act(p => p, actionCapacity);
                };

                it["should configure outgoing flow for some actions in a pipeline"] = () =>
                {
                    //todo an action should be able to emit messages in-flight while not yet finished
                };
            }
        }

        public class MessageFlowPropagation: NSpec
        {
            public void describe_message_flow_propagation()
            {
                it["should configure echo response on incoming flow"] = () =>
                {
                    MessageFlowFactory.Build()
                        .On<Payload>("incoming_label")
                        .Respond();
                };

                it["should configure direct forward of incoming flow"] = () =>
                {
                    MessageFlowFactory.Build()
                        .On<Payload>("incoming_label")
                        .Forward("outgoing_label");
                };

                it["should configure action response on incoming flow"] = () =>
                {
                    MessageFlowFactory.Build()
                        .On<Payload>("incoming_label")
                        .Act(p => new NewPayload())
                        .Respond();
                };
                
                it["should configure action result forwarding"] = () =>
                {
                    MessageFlowFactory.Build()
                        .On<Payload>("incoming_label")
                        .Act(p => new NewPayload())
                        .Forward("outgoing_label");

                    MessageFlowFactory.Build()
                        .On<Payload>("incoming_label")
                        .Act(p => new NewPayload())
                        .Forward("outgoing_label");
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

                    MessageFlowFactory.Build()
                        .On<Payload>("incoming_label")
                        .Act(p => new NewPayload())
                        .Cache<Payload, NewPayload>(cachePolicy);
                };

                it["should configure action result caching and responding"] = () =>
                {
                    var cachePolicy  = new DefaultCachePolicy(TimeSpan.FromSeconds(10));

                    MessageFlowFactory.Build()
                        .On<Payload>("incoming_label")
                        .Act(p => new NewPayload())
                        .Cache<Payload, NewPayload>(cachePolicy)
                        .Respond();
                };

                it["should configure action result caching and forwarding"] = () =>
                {
                    var cachePolicy = new DefaultCachePolicy(TimeSpan.FromSeconds(10));

                    MessageFlowFactory.Build()
                        .On<Payload>("incoming_label")
                        .Act(p => new NewPayload())
                        .Cache<Payload, NewPayload>(cachePolicy)
                        .Forward("outgoing_label");
                };
            }
        }
    }
}
