using Contour.Transport.RabbitMq.Topology;

using NUnit.Framework;

namespace Contour.Common.Tests
{
    class QueueBuilderSpecs
    {
        /// <summary>
        /// When using limit of queue
        /// </summary>
        [TestFixture]
        [Category("Unit")]
        public class WhenUsingLimitOfQueue
        {
            /// <summary>
            /// Then should setub limit
            /// </summary>
            [Test]
            public void ShouldSetupLimit()
            {
                QueueBuilder sut = new QueueBuilder("queue").WithLimit(1000);
                Assert.AreEqual(1000, sut.Instance.Limit, "Should be set correct limit of message amount.");
            }
        }
    }
}
