using System;
using System.Threading;
using System.Threading.Tasks;
using Contour.Flow.Configuration;
using Contour.Flow.Transport;

namespace Contour.Tests
{
    public class RequestResponseSpecs : NSpec
    {
        public void describe_flow()
        {
            it["should execute client callback on response"] = () =>
            {
                var tcs = new TaskCompletionSource<bool>();
                var transport = new LocalFlowTransport();
                var broker = new FlowBroker();
                broker.Register("local", transport);

                var flow = broker.Create<int>("local")
                    .On("label")
                    .Act(p => new NewPayload() {Value = 3})
                    .Act(p => "abc")
                    .Respond();

                var entry = flow.Entry(ctx => tcs.SetResult(true));
                entry.Post(123);

                tcs.Task.Wait();

                Thread.Sleep(TimeSpan.FromMinutes(10));
            };
        }
    }
}