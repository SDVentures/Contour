using System;
using System.Threading;
using System.Threading.Tasks;
using Contour.Flow.Configuration;
using Contour.Flow.Execution;
using Contour.Flow.Transport;
using FakeItEasy;

namespace Contour.Tests
{
    public class RequestResponseSpecs : NSpec
    {
        public void describe_flow_callback_invocation()
        {
            it["should execute client callback on response"] = () =>
            {
                var tcs = new TaskCompletionSource<bool>();
                var transport = new LocalFlowTransport();
                var broker = new FlowBroker();
                broker.Register("local", transport);

                var flow = broker.Create<int>("local")
                    .On("label")
                    .Act(p => p.In * 2)
                    .Respond();

                var entry = flow.Entry(ctx => tcs.SetResult(true));
                entry.Post(123);

                tcs.Task.Wait();
            };

            it["should correlate client callbacks"] = () =>
            {
                var tcs1 = new TaskCompletionSource<bool>();
                var tcs2 = new TaskCompletionSource<bool>();
                
                var transport = new LocalFlowTransport();
                var broker = new FlowBroker();
                broker.Register("local", transport);

                var flow = broker.Create<int>("local")
                    .On("label")
                    .Act(p => p.In * 2)
                    .Respond();

                var action1 = A.Fake<Action<FlowContext<FlowContext<int>, int>>>();
                var a = A.CallTo(action1).Invokes(() => tcs1.SetResult(true));
                
                var action2 = A.Fake<Action<FlowContext<FlowContext<int>, int>>>();
                var b = A.CallTo(action2).Invokes(() => tcs2.SetResult(true));

                var firstEntry = flow.Entry(action1);
                var secondEntry = flow.Entry(action2);

                firstEntry.Post(1);
                tcs1.Task.Wait();

                a.MustHaveHappened();
                b.MustNotHaveHappened();

                secondEntry.Post(2);
                tcs2.Task.Wait();

                b.MustHaveHappened();
            };
        }
    }
}