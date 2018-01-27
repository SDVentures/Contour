namespace Contour.Caching.Tests.TestImplementations
{
    using System;

    using Contour.Receiving;

    internal class TestContext : IConsumingContext<TestRequest>
    {
        public TestContext(Message<TestRequest> message)
        {
            this.Message = message;
        }

        public TestResponse Response { get; private set; }

        public IBusContext Bus
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool CanReply => true;

        public Message<TestRequest> Message { get; }

        public void Reply<TResponse>(TResponse response, Expires expires = null) where TResponse : class
        {
            this.Response = (TestResponse)Convert.ChangeType(response, typeof(TestResponse));
        }

        public void Accept()
        {
            throw new NotImplementedException();
        }

        public void Forward(string label)
        {
            throw new NotImplementedException();
        }

        public void Forward(MessageLabel label)
        {
            throw new NotImplementedException();
        }

        public void Forward<TOut>(string label, TOut payload = null) where TOut : class
        {
            throw new NotImplementedException();
        }

        public void Forward<TOut>(MessageLabel label, TOut payload = null) where TOut : class
        {
            throw new NotImplementedException();
        }

        public void Reject(bool requeue)
        {
            throw new NotImplementedException();
        }
    }
}