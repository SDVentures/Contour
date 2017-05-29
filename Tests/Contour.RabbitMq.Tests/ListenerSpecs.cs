using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Contour.Testing.Transport.RabbitMq;
using FluentAssertions;
using NUnit.Framework;
// ReSharper disable InconsistentNaming

namespace Contour.RabbitMq.Tests
{
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here."), SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    public class ListenerSpecs
    {
        [TestFixture]
        [Category("Manual")]
        public class given_a_high_parallelism_level: RabbitMqFixture
        {
            [SetUp]
            public override void SetUp()
            {
                base.SetUp();
                this.Broker.DropConnections();
            }

            [Test]
            public void should_start_consuming_quickly()
            {
                var StartupTime = TimeSpan.FromMinutes(1);
                const int Level = 5000;

                this.StartBus("Consumer",
                    cfg => cfg.On<BooMessage>("boo").ReactWith((m, c) => { }).WithParallelismLevel(Level));

                Thread.Sleep(StartupTime);

                var connections = this.Broker.GetConnections().ToList();
                connections.Should().HaveCount(1);

                var connection = connections.First();
                connection.channels.Should().Be(Level + 2); // two more channels are created for outgoing fault labels
            }
        }
    }
}
