using System.Collections;

namespace Contour.Transport.RabbitMQ.Internal
{
    internal class RoundRobinSelector : IProducerSelector
    {
        private IList items;
        private IEnumerator enumerator;
        
        public void Initialize(IList list)
        {
            this.items = list;
            this.enumerator = this.items.GetEnumerator();
        }

        public int Next()
        {
            while (true)
            {
                if (this.enumerator.MoveNext())
                {
                    return this.items.IndexOf(this.enumerator.Current);
                }

                this.enumerator.Reset();
            }
        }
    }
}