using System;
using System.Threading.Tasks.Dataflow;
using Contour.Flow;
using Contour.Flow.Configuration;

namespace Contour.Tests
{
    public class TplPrototypeSpec: NSpec
    {
        public class Payload
        {
        }

        public class NewPayload
        {
        }

        public void describe_tpl_prototype()
        {
            context["should provide a TPL-block to consume messages of different types"] = () =>
            {
                it["should consume messages in a TPL-block"] = () =>
                {

                };

                it["should consume messages of single type"] = () =>
                {

                };

                it["should consume messages of different types"] = () =>
                {

                };
            };

            context["should provide a TPL-block to produce messages"] = () =>
            {
                it["should produce a message given a specific label (publish)"] = () =>
                {

                };

                it["should produce a message as a reply (response)"] = () =>
                {

                };
            };
        }

        public void describe_flow_configuration()
        {
            context["should provide fluent configuration options"] = () =>
            {
                it["should configure incoming flow"] = () =>
                {
                    var flow = FlowFactory.Build()
                        .On<Payload>("incoming_label");
                };

                it["should configure incoming flow buffering"] = () =>
                {
                    const int capacity = 10;
                    var flow = FlowFactory.Build()
                        .On<Payload>("incoming_label", capacity);
                };

                it["should configure incoming flow single action"] = () =>
                {
                    var flow = FlowFactory.Build()
                        .On<Payload>("incoming_label")
                        .Act<Payload, Payload>(p => p);
                };

                it["should configure incoming flow multiple actions"] = () =>
                {
                    var flow = FlowFactory.Build()
                        .On<Payload>("incoming_label")
                        .Act<Payload, Payload>(p => p)
                        .Act<Payload, Payload>(p2 => p2);
                };

                it["should configure incoming flow action scalability"] = () =>
                {
                    var flow = FlowFactory.Build()
                        .On<Payload>("incoming_label")
                        .Act<Payload, Payload>(p => p);
                };

                it["should configure incoming flow action buffering"] = () =>
                {
                    
                };

                it["should configure direct response on incoming flow"] = () =>
                {
                    var flow = FlowFactory.Build()
                        .On<Payload>("incoming_label")
                        .Respond<string, bool>((p) => true);
                };

                it["should configure action response on incoming flow"] = () =>
                {
                    var flow = FlowFactory.Build()
                        .On<Payload>("incoming_label")
                        .Act<Payload, NewPayload>(p => new NewPayload())
                        .Respond();
                };

                it["should configure response caching"] = () =>
                {
                    var flow = FlowFactory.Build()
                        .On<Payload>("incoming_label")
                        .Act<Payload, NewPayload>(p => new NewPayload())
                        .Respond()
                        .CacheFor(TimeSpan.FromMinutes(1));
                };

                it["should configure direct forward of incoming flow"] = () =>
                {
                    var flow = FlowFactory.Build()
                        .On<Payload>("incoming_label")
                        .Forward("outgoing_label");
                };

                it["should configure action result forward of incoming flow"] = () =>
                {
                    var flow = FlowFactory.Build()
                        .On<Payload>("incoming_label")
                        .Act<Payload, Payload>((p) => p)
                        .Forward("outgoing_label");
                };
            };
        }
    }
}
