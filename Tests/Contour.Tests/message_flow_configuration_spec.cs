using System;
using Contour.Flow.Configuration;
using FluentAssertions;

namespace Contour.Tests
{
    // ReSharper disable once InconsistentNaming
    public class message_flow_configuration_spec: NSpec
    {
        public void describe_message_flow_configuration()
        {
            context["should provide fluent configuration options"] = () =>
            {
                it["should configure incoming flow"] = () =>
                {
                    MessageFlowFactory.Build()
                        .On<Payload>("incoming_label")
                        .Should()
                        .BeAssignableTo<IActingFlow<Payload>>();
                };

                it["should configure incoming flow buffering"] = () =>
                {
                    const int capacity = 10;
                    MessageFlowFactory.Build()
                        .On<Payload>("incoming_label", capacity)
                        .Should()
                        .BeAssignableTo<IActingFlow<Payload>>();
                };

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
                        .Act(p => p, capacity: actionCapacity);
                };

                it["should configure direct echo response on incoming flow"] = () =>
                {
                    MessageFlowFactory.Build()
                        .On<Payload>("incoming_label")
                        .Respond<Payload>();
                };

                it["should configure direct response on incoming flow"] = () =>
                { };

                it["should configure action response on incoming flow"] = () =>
                {
                    MessageFlowFactory.Build()
                        .On<Payload>("incoming_label")
                        .Act(p => new NewPayload())
                        .Respond<Payload>();
                };

                it["should configure response caching"] = () =>
                {
                    var duration = TimeSpan.FromSeconds(10);

                    MessageFlowFactory.Build()
                        .On<Payload>("incoming_label")
                        .Act(p => new NewPayload())
                        .Cache<NewPayload>(duration)
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
                        .Act(p => p)
                        .Forward("outgoing_label");
                };
            };
        }
    }
}
