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

            var pool = new RabbitConnectionPool((RabbitBus)bus, 1);
            var source = new CancellationTokenSource();
            pool.Get(source.Token);
            pool.Drop();
            Assert.IsTrue(pool.Count == 0);

            pool.Get(source.Token);
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

            var pool = new RabbitConnectionPool((RabbitBus)bus, Count);

            var i = 0;
            var source = new CancellationTokenSource();
            var connections = new List<IConnection>();

            while (i++ < Count)
            {
                var con = pool.Get(source.Token);
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
            var bus = this.ConfigureBus(
                   "Test",
                   cfg =>
                   {
                       cfg.SetConnectionString("amqp://10.10.10.10/integration");
                       cfg.Route("some.label");
                    });

            const int Count = 5;
            var pool = new RabbitConnectionPool((RabbitBus)bus, Count);
            var source = new CancellationTokenSource();
            
            // ReSharper disable once MethodSupportsCancellation
            var task = Task.Factory.StartNew(
                () =>
                    {
                        var con = pool.Get(source.Token);
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