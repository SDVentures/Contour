using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Contour.Flow.Execution;
using Contour.Flow.Transport;

namespace Contour.Flow.Configuration
{
    public class RequestFlow<TInput, TOutput>: IRequestFlow<TInput>
    {
        private readonly Guid id = Guid.NewGuid();
        private readonly IPropagatorBlock<TInput, FlowContext<TInput>> propagator;

        public string Label { get; }
        public Type Type { get; }
        public IFlowRegistry Registry { get; set; }
        public IFlowTransport Transport { get; }

        public RequestFlow(ISourceBlock<TOutput> source, Action<TOutput> callback)
        {
            var flow = (IMessageFlow<FlowContext<TInput>>) Registry.Get(this.Label);
            var targetFlow = flow.AsTarget();

            propagator = new TransformBlock<TInput, FlowContext<TInput>>(input => new FlowContext<TInput>()
            {
                Id = this.id,
                In = input
            });

            propagator.LinkTo(targetFlow);

            var correlationQuery = new Predicate<TOutput>(p =>
            {
                var ctx = p as FlowContext;
                var sourceCtx = ctx.Unwind();
                return (sourceCtx.Id == this.id);
            });
            var action = new ActionBlock<TOutput>(callback);
            source.LinkTo(action, correlationQuery);
        }

        public bool Post(TInput message)
        {
            return propagator.Post(message);
        }

        public Task<bool> PostAsync(TInput message)
        {
            throw new NotImplementedException();
        }

        public Task<bool> PostAsync(TInput message, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}