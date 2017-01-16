using System.Threading.Tasks.Dataflow;

namespace Contour.Flow.Configuration
{
    internal class OutgoingFlow: IOutgoingFlow
    {
        private readonly IDataflowBlock source;

        public OutgoingFlow(IDataflowBlock source)
        {
            this.source = source;
        }

        public void Respond()
        {
            throw new System.NotImplementedException();
        }

        public void Forward(string label)
        {
            throw new System.NotImplementedException();
        }
    }
}