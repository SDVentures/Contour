// ReSharper disable InconsistentNaming



namespace Contour.Common.Tests
{
    using System.Collections.Generic;
    using Moq;
    using NUnit.Framework;

    public class ConnectionPoolSpecs
    {
        [TestFixture]
        [Category("Unit")]
        public class when_declaring_connection_pool
        {
            internal class FakeConnectionPool : ConnectionPool<IConnection>
            {
                public FakeConnectionPool(IConnectionProvider<IConnection> provider, int maxSize) : base(maxSize)
                {
                    this.Provider = provider;
                }
            }

            [Test]
            public void should_use_positive_size_limit_if_provided()
            {
                var size = 1;
                var pool = new FakeConnectionPool(null, size);
                Assert.IsTrue(pool.MaxSize == size);
            }

            [Test]
            public void should_use_max_size_limit_if_zero_or_negative_is_provided()
            {
                var pool = new FakeConnectionPool(null, -1);
                Assert.IsTrue(pool.MaxSize == int.MaxValue);

                pool = new FakeConnectionPool(null, 0);
                Assert.IsTrue(pool.MaxSize == int.MaxValue);
            }

            [Test]
            public void should_create_new_connections_until_max_size_reached()
            {
                const int size = 3;

                var provider = new Mock<IConnectionProvider<IConnection>>();
                provider.Setup(cp => cp.Create()).Returns(() => new Mock<IConnection>().Object);

                var list = new List<IConnection>();

                var pool = new FakeConnectionPool(provider.Object, size);
                for (var i = 0; i < size; i++)
                {
                    var con = pool.Get();
                    Assert.IsFalse(list.Contains(con));

                    list.Add(con);
                }
            }

            [Test]
            public void should_reuse_existing_connections_when_max_size_reached()
            {
                const int size = 3;

                var provider = new Mock<IConnectionProvider<IConnection>>();
                provider.Setup(cp => cp.Create()).Returns(() => new Mock<IConnection>().Object);

                var list = new List<IConnection>();

                var pool = new FakeConnectionPool(provider.Object, size);
                for (var i = 0; i < 2 * size; i++)
                {
                    var con = pool.Get();
                    if (i < size)
                    {
                        Assert.IsFalse(list.Contains(con));
                        list.Add(con);
                    }
                    else
                    {
                        Assert.IsTrue(list.Contains(con));
                    }
                }

                Assert.IsTrue(pool.Count == size);
            }
        }
    }
}
