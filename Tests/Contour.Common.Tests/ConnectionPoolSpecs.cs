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
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here."), SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here."), TestFixture]
        [Category("Unit")]
        public class when_declaring_connection_pool
        {
            [Test]
            public void should_dispose_connections_on_pool_disposal()
            {
                var conString = string.Empty;
                var connection = new Mock<IConnection>();

                var provider = new Mock<IConnectionProvider<IConnection>>();
                provider.Setup(cp => cp.Create(conString)).Returns(() => connection.Object);
                
                var pool = new FakeConnectionPool(provider.Object);
                pool.Get(conString, false, CancellationToken.None);
                pool.Dispose();

                connection.Verify(c => c.Dispose());
            }

            [Test]
            public void should_throw_if_disposed_pool_is_accessed()
            {
                var conString = string.Empty;
                var connection = new Mock<IConnection>();

                var provider = new Mock<IConnectionProvider<IConnection>>();
                provider.Setup(cp => cp.Create(conString)).Returns(() => connection.Object);

                var pool = new FakeConnectionPool(provider.Object);
                pool.Dispose();
                try
                {
                    pool.Get(conString, false, CancellationToken.None);
                    Assert.Fail("Should throw if a disposed pool is accessed");
                }
                catch (Exception)
                {
                }
            }

            [Test]
            public void should_close_and_dispose_all_connections_on_pool_drop()
            {
                var conString = string.Empty;
                var cons = new List<Mock<IConnection>>();
                var provider = new Mock<IConnectionProvider<IConnection>>();
                provider.Setup(cp => cp.Create(conString)).Returns(() =>
                {
                    var con = new Mock<IConnection>();
                    cons.Add(con);
                    return con.Object;
                });

                const int Count = 5;
                var pool = new FakeConnectionPool(provider.Object);

                var i = 0;
                while (i++ < Count)
                {
                    pool.Get(conString, false, CancellationToken.None);
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
                var conString = string.Empty;
                var provider = new Mock<IConnectionProvider<IConnection>>();
                provider.Setup(cp => cp.Create(conString)).Returns(() =>
                {
                    var con = new Mock<IConnection>();
                    return con.Object;
                });

                var pool = new FakeConnectionPool(provider.Object);

                var source = new CancellationTokenSource();
                source.Cancel();

                Assert.Throws<OperationCanceledException>(() => pool.Get(conString, false, source.Token));
            }

            [Test]
            public void should_create_and_use_connection_if_reusable_for_empty_pool_group()
            {
                var conString = string.Empty;
                var provider = new Mock<IConnectionProvider<IConnection>>();
                provider.Setup(cp => cp.Create(conString)).Returns(() =>
                {
                    var con = new Mock<IConnection>();
                    return con.Object;
                });

                var pool = new FakeConnectionPool(provider.Object);
                var connection = pool.Get(conString, true, CancellationToken.None);

                Assert.IsNotNull(connection);
            }

            [Test]
            public void should_reuse_connections_in_group_if_reusable()
            {
                var conString = string.Empty;
                var provider = new Mock<IConnectionProvider<IConnection>>();
                provider.Setup(cp => cp.Create(conString)).Returns(() =>
                {
                    var con = new Mock<IConnection>();
                    return con.Object;
                });

                var pool = new FakeConnectionPool(provider.Object);
                var c1 = pool.Get(conString, true, CancellationToken.None);
                var c2 = pool.Get(conString, true, CancellationToken.None);

                Assert.IsTrue(c1 == c2);
            }

            [Test]
            public void should_not_reuse_connections_in_group_if_not_reusable()
            {
                var conString = string.Empty;
                var provider = new Mock<IConnectionProvider<IConnection>>();
                provider.Setup(cp => cp.Create(conString)).Returns(() =>
                {
                    var con = new Mock<IConnection>();
                    return con.Object;
                });

                var pool = new FakeConnectionPool(provider.Object);
                var c1 = pool.Get(conString, false, CancellationToken.None);
                var c2 = pool.Get(conString, false, CancellationToken.None);

                Assert.IsTrue(c1 != c2);
            }

            [Test]
            public void should_not_reuse_connections_marked_as_non_reusable()
            {
                var conString = string.Empty;
                var provider = new Mock<IConnectionProvider<IConnection>>();
                provider.Setup(cp => cp.Create(conString)).Returns(() =>
                {
                    var con = new Mock<IConnection>();
                    return con.Object;
                });

                var pool = new FakeConnectionPool(provider.Object);
                var c1 = pool.Get(conString, false, CancellationToken.None);
                var c2 = pool.Get(conString, true, CancellationToken.None);

                Assert.IsTrue(c1 != c2);

                var c3 = pool.Get(conString, true, CancellationToken.None);
                Assert.IsTrue(c1 != c3);
                Assert.IsTrue(c2 == c3);
            }

            [Test]
            public void should_not_reuse_connections_in_different_groups()
            {
                const string ConnectionString1 = "1";
                const string ConnectionString2 = "2";

                var provider = new Mock<IConnectionProvider<IConnection>>();
                provider.Setup(cp => cp.Create(It.IsAny<string>())).Returns(() =>
                {
                    var con = new Mock<IConnection>();
                    return con.Object;
                });

                var pool = new FakeConnectionPool(provider.Object);
                var c1 = pool.Get(ConnectionString1, true, CancellationToken.None);
                var c2 = pool.Get(ConnectionString2, true, CancellationToken.None);

                Assert.False(c1 == c2);
            }
        }
    }
}
