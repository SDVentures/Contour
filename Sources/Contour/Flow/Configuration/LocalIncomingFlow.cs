using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Contour.Flow.Transport;

namespace Contour.Flow.Configuration
{
    public class LocalIncomingFlow<TInput> : IIncomingFlow<TInput>
    {
        private readonly ITargetBlock<TInput> buffer;
        public string Label { get; }
        public Type Type { get; }
        public IFlowRegistry Registry { get; set; }
        public IFlowTransport Transport { get; }

        public LocalIncomingFlow(ITargetBlock<TInput> buffer)
        {
            this.buffer = buffer;
        }

        public bool Post(TInput message)
        {
            return buffer.Post(message);
        }

        public Task<bool> PostAsync(TInput message)
        {
            return buffer.SendAsync(message);
        }

        public Task<bool> PostAsync(TInput message, CancellationToken token)
        {
            return buffer.SendAsync(message, token);
        }
    }
}