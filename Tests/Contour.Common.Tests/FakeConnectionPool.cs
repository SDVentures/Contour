namespace Contour.Common.Tests
{
    internal class FakeConnectionPool : ConnectionPool<IConnection>
    {
        public FakeConnectionPool(IConnectionProvider<IConnection> provider, int maxSize) : base(maxSize)
        {
            this.Provider = provider;
        }
    }
}