// ReSharper disable InconsistentNaming

namespace Contour.Common.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;

    using Moq;
    using NUnit.Framework;

    /// <summary>
    /// Connection pool specifications
    /// </summary>
    public class ConnectionPoolSpecs
    {
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here."),SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here."), TestFixture]
        [Category("Unit")]
        public class when_declaring_connection_pool
        {
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
                    var source = new CancellationTokenSource();
                    var con = pool.Get(source.Token);
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
                    var source = new CancellationTokenSource();
                    var con = pool.Get(source.Token);
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

            [Test]
            public void should_dispose_connections_on_pool_disposal()
            {
                var connection = new Mock<IConnection>();

                var provider = new Mock<IConnectionProvider<IConnection>>();
                provider.Setup(cp => cp.Create()).Returns(() => connection.Object);
                
                var pool = new FakeConnectionPool(provider.Object, 1);
                var source = new CancellationTokenSource();
                pool.Get(source.Token);
                pool.Dispose();
                
                connection.Verify(c=> c.Dispose());
            }

            [Test]
            public void should_throw_if_disposed_pool_is_accessed()
            {
                var connection = new Mock<IConnection>();

                var provider = new Mock<IConnectionProvider<IConnection>>();
                provider.Setup(cp => cp.Create()).Returns(() => connection.Object);

                var pool = new FakeConnectionPool(provider.Object, 1);
                pool.Dispose();
                try
                {
                    var source = new CancellationTokenSource();
                    pool.Get(source.Token);
                    Assert.Fail("Should throw if a disposed pool is accessed");
                }
                catch (Exception)
                {
                }
            }

            [Test]
            public void should_close_and_dispose_all_connections_on_pool_drop()
            {
                var cons = new List<Mock<IConnection>>();
                var provider = new Mock<IConnectionProvider<IConnection>>();
                provider.Setup(cp => cp.Create()).Returns(() =>
                {
                    var con = new Mock<IConnection>();
                    cons.Add(con);
                    return con.Object;
                });

                const int count = 5;
                var pool = new FakeConnectionPool(provider.Object, count);

                var i = 0;
                var source = new CancellationTokenSource();
                while (i++ < count)
                {
                    pool.Get(source.Token);
                }

                pool.Drop();

                while (cons.Any())
                {
                    var con = cons.First();
                    con.Verify(c => c.Close());
                    con.Verify(c => c.Dispose());

                    cons.Remove(con);
                }
            }

            [Test]
            public void should_cancel_not_started_get_operation()
            {
                var provider = new Mock<IConnectionProvider<IConnection>>();
                provider.Setup(cp => cp.Create()).Returns(() =>
                {
                    var con = new Mock<IConnection>();
                    return con.Object;
                });

                var pool = new FakeConnectionPool(provider.Object, 1);

                var source = new CancellationTokenSource();
                source.Cancel();

                var result = pool.Get(source.Token);
                Assert.IsTrue(result == null);
            }
        }
    }
}
