using System;
using Contour.Flow.Configuration;
// ReSharper disable InconsistentNaming

namespace Contour.Tests
{
    public class message_flow_configuration
    {
        public class incoming_message_flow : NSpec
        {
            public void describe_incoming_message_flow()
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

        public class message_flow_actions : NSpec
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
            }
        }

        public class message_flow_propagation: NSpec
        {
            public void describe_message_flow_propagation()
            {
                it["should configure echo response on incoming flow"] = () =>
                {
                    MessageFlowFactory.Build()
                        .On<Payload>("incoming_label")
                        .Respond();
                };

                it["should configure action response on incoming flow"] = () =>
                {
                    MessageFlowFactory.Build()
                        .On<Payload>("incoming_label")
                        .Act(p => new NewPayload())
                        .Respond();
                };

                it["should configure direct forward of incoming flow"] = () =>
                {
                    MessageFlowFactory.Build()
                        .On<Payload>("incoming_label")
                        .Forward("outgoing_label");
                };

                it["should configure action result forward of incoming flow"] = () =>
                {
                    MessageFlowFactory.Build()
                        .On<Payload>("incoming_label")
                        .Act(p => new NewPayload())
                        .Forward("outgoing_label");
                };
            }
        }

        public class message_flow_caching : NSpec
        {
            public void describe_message_flow_caching()
            {
                it["should configure response caching"] = () =>
                {
                    var duration = TimeSpan.FromSeconds(10);

                    MessageFlowFactory.Build()
                        .On<Payload>("incoming_label")
                        .Act(p => new NewPayload())
                        .Cache(duration)
                        .Respond();
                };
            }
        }
    }
}
