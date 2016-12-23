using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Contour.Flow.Configuration;
using FakeItEasy;

namespace Contour.Tests
{
    // ReSharper disable once InconsistentNaming
    public class message_flow_handling : NSpec
    {
        public void describe_flow()
        {
            it["should buffer incoming messages"] = () =>
            {
                BufferBlock<Payload> buffer;
                var factory = GetMessageFlowFactory(out buffer);

                factory.Build().On<Payload>("incoming_label");
                buffer.Post(new Payload());
            };

            it["should perform single action on incoming message"] = () =>
            {
                BufferBlock<Payload> buffer;
                var factory = GetMessageFlowFactory(out buffer);

                var action = A.Fake<Func<Payload, Payload>>();
                factory.Build()
                    .On<Payload>("incoming_label")
                    .Act(action);

                var tcs = new TaskCompletionSource<bool>();
                A.CallTo(action).Invokes(() => tcs.SetResult(true));
                buffer.Post(new Payload());

                tcs.Task.Wait(AMinute());
                A.CallTo(action).MustHaveHappened();
            };

            it["should perform multiple actions on incoming message"] = () =>
            {
                BufferBlock<Payload> buffer;
                var factory = GetMessageFlowFactory(out buffer);

                var action1 = A.Fake<Func<Payload, Payload>>();
                var action2 = A.Fake<Func<Payload, Payload>>();

                factory.Build()
                    .On<Payload>("incoming_label")
                    .Act(action1)
                    .Act(action2);

                var tcs1 = new TaskCompletionSource<bool>();
                var tcs2 = new TaskCompletionSource<bool>();

                A.CallTo(action1).Invokes(() => tcs1.SetResult(true));
                A.CallTo(action2).Invokes(() => tcs2.SetResult(true));

                buffer.Post(new Payload());

                tcs1.Task.Wait(AMinute());
                A.CallTo(action1).MustHaveHappened();

                tcs2.Task.Wait(AMinute());
                A.CallTo(action2).MustHaveHappened();
            };

            it["should respond with echo"] = () =>
            {
                BufferBlock<Payload> buffer;
                var factory = GetMessageFlowFactory(out buffer);
                
            };
        }

        public void describe_flow_management()
        {
            it["should manage the time a message stays in client's buffer"] = () => { };
        }

        private static IMessageFlowFactory GetMessageFlowFactory(out BufferBlock<Payload> buffer)
        {
            var factory = A.Fake<IMessageFlowFactory>();
            buffer = new BufferBlock<Payload>();
            A.CallTo(() => factory.Build()).Returns(new MessageFlow(buffer));
            return factory;
        }

        private static TimeSpan AMinute()
        {
            return TimeSpan.FromMinutes(1);
        }
    }
}