// ReSharper disable InconsistentNaming
namespace Contour.RabbitMq.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using Testing.Transport.RabbitMq;
    using Transport.RabbitMQ.Internal;
    using Transport.RabbitMQ.Topology;

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here."), SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here."), TestFixture]
    [Category("Integration")]
    public class ConnectionPoolSpecs : RabbitMqFixture
    {
        public void should_create_connections_after_drop()
        {
            var bus = this.ConfigureBus(
                   "Test",
                   cfg =>
                   {
                       cfg.On<BooMessage>("one")
                           .ReactWith(m => { })
                           .WithEndpoint(
                               builder => builder.ListenTo(builder.Topology.Declare(Queue.Named("one.queue"))));
                   });

            var pool = new RabbitConnectionPool(bus);
            var source = new CancellationTokenSource();
            var conString = string.Empty;

            pool.Get(conString, false, source.Token);
            pool.Drop();
            Assert.IsTrue(pool.Count == 0);

            pool.Get(conString, false, source.Token);
            Assert.IsTrue(pool.Count == 1);
        }

        [Test]
        public void should_remove_disposed_connections()
        {
            const int Count = 5;
            var bus = this.ConfigureBus(
                   "Test",
                   cfg =>
                   {
                       cfg.On<BooMessage>("one")
                           .ReactWith(m => { })
                           .WithEndpoint(
                               builder => builder.ListenTo(builder.Topology.Declare(Queue.Named("one.queue"))));
                   });

            var pool = new RabbitConnectionPool(bus);

            var i = 0;
            var conString = bus.Configuration.ConnectionString;
            var source = new CancellationTokenSource();
            var connections = new List<IConnection>();

            while (i++ < Count)
            {
                var con = pool.Get(conString, false, source.Token);
                connections.Add(con);
            }

            i = 0;
            while (connections.Any())
            {
                var con = connections.First();
                con.Dispose();
                connections.Remove(con);

                Assert.IsTrue(pool.Count == Count - ++i);
            }
        }

        [Test]
        public void should_cancel_running_get_operation()
        {
            const string ConString = "amqp://10.10.10.10/integration";

            var bus = this.ConfigureBus(
                   "Test",
                   cfg =>
                   {
                       cfg.SetConnectionString(ConString);
                       cfg.Route("some.label");
                    });

            var pool = new RabbitConnectionPool(bus);
            var source = new CancellationTokenSource();
            
            // ReSharper disable once MethodSupportsCancellation
            var task = Task.Factory.StartNew(
                () =>
                {
                    var con = pool.Get(ConString, false, source.Token);
                    return con;
                });
            
            task.Wait(TimeSpan.FromSeconds(10));
            source.Cancel();

            if (task.Wait(TimeSpan.FromSeconds(10)))
            {
                Assert.IsTrue(task.Result != null);
            }
            else
            {
                Assert.Fail("Operation has not been canceled");
            }
        }
    }
}