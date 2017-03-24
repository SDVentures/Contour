namespace Contour.Common.Tests
{
    internal class FakeConnectionPool : ConnectionPool<IConnection>
    {
        public FakeConnectionPool(IConnectionProvider<IConnection> provider)
        {
            this.Provider = provider;
        }
    }
}