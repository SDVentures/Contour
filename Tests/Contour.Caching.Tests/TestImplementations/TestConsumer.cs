namespace Contour.Caching.Tests.TestImplementations
{
    using System.Collections.Generic;

    using Contour.Receiving;
    using Contour.Receiving.Consumers;

    internal class TestConsumer : IConsumerOf<TestRequest>
    {
        public IDictionary<int, string> DataSource { get; set; } = new Dictionary<int, string>();

        public void InitData()
        {
            this.DataSource[0] = "Zero";
            this.DataSource[1] = "One";
            this.DataSource[2] = "Two";
            this.DataSource[3] = "Three";
        }

        public void Handle(IConsumingContext<TestRequest> context)
        {
            var id = context.Message.Payload.Id;

            var value = this.DataSource.ContainsKey(id) ? this.DataSource[id] : null;

            context.Reply(new TestResponse { Value = value });
        }
    }
}