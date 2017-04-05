namespace Contour.RabbitMq.Tests
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// The message attribute specs.
    /// </summary>
    // ReSharper disable InconsistentNaming
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
    public class MessageAttributeSpecs
    {
        /// <summary>
        /// The zoo.
        /// </summary>
        [Message("zoo.msg")]
        public class Zoo
        {
            /// <summary>
            /// Gets or sets the content.
            /// </summary>
            public string Content { get; set; }
        }

        /*
		[TestFixture]
		[Category("Integration")]
		public class when_publishing_message_with_message_attribute : RabbitMqFixture
		{
			[Test]
			public void should_catch_message_on_subscribed_consumer()
			{
				var result = "";
				var waitHandle = new AutoResetEvent(false);

				var producer = StartProducer(cfg => cfg.Route<Zoo>());

				StartConsumer(cfg => cfg.On<Zoo>().ReactWith((m, ctx) =>
				{
					result = m.Content;
					waitHandle.Set();
				}));

				producer.Emit(new Zoo { Content = "this" });

				waitHandle.WaitOne(5.Seconds()).Should().BeTrue();

				result.Should().Be("this");
			}
		}
*/

        /*
		[TestFixture]
		[Category("Integration")]
		public class when_publishing_message_missing_message_attribute : RabbitMqFixture
		{
			[Test]
			public void should_throw_bus_configuration_exception()
			{
				var producer = StartProducer(cfg => cfg.Route<Zoo>());

				StartConsumer(cfg => cfg.On<Zoo>().ReactWith(_ => {}));

				producer
					.Invoking(p => p.Emit(new { Content = "this" }))
					.ShouldThrow<BusConfigurationException>();
			}
		}
*/
    }

    // ReSharper restore InconsistentNaming
}
