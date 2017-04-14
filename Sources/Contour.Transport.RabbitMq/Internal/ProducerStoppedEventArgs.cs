using System;

namespace Contour.Transport.RabbitMq.Internal
{
    internal class ProducerStoppedEventArgs : EventArgs
    {
        public ProducerStoppedEventArgs(IProducer producer, OperationStopReason reason)
        {
            this.Producer = producer;
            this.Reason = reason;
        }

        public IProducer Producer
        {
            get;
        }

        public OperationStopReason Reason
        {
            get;
        }
    }
}