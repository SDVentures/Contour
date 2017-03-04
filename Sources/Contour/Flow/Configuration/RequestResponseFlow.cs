using System;
using System.Threading.Tasks.Dataflow;
using Contour.Flow.Execution;

namespace Contour.Flow.Configuration
{
    /// <summary>
    /// In a request-response scenario two things need to be set up:
    /// 1. if a client has registered a callback then a an action block needs to be attached to the flow tail broadcast block
    /// 2. each entry point created by the client needs to have a unique id to send requests and be used as correlation key to return responses
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public class RequestResponseFlow<TInput, TOutput> : IRequestResponseFlow<TInput, TOutput>
    {
        private readonly ISourceBlock<TOutput> tailBlock;

        public IFlowRegistry Registry { private get; set; }
        public string Label { private get; set; }

        public RequestResponseFlow(ISourceBlock<TOutput> tailBlock)
        {
            this.tailBlock = tailBlock;
        }

        public IFlowEntry<TInput> Entry()
        {
            throw new NotImplementedException();
        }

        public IFlowEntry<TInput> Entry(Action<TOutput> callback)
        {
            var head = (IMessageFlow<TInput>)this.Registry.Get(this.Label);
            var entry = head.Entry();
            
            var correlationQuery = new Predicate<TOutput>(p =>
            {
                var ctx = p as FlowContext;
                var sourceCtx = ctx?.Unwind();
                return sourceCtx?.Id == entry.Id;
            });

            var action = new ActionBlock<TOutput>(callback);
            tailBlock.LinkTo(action, correlationQuery);

            return entry;
        }
    }
}