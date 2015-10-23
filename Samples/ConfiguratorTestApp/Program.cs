// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="">
//   
// </copyright>
// <summary>
//   The program.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ConfiguratorTestApp
{
    using System;
    using System.Threading;

    using Common.Logging;
    using Common.Logging.Simple;

    using Contour;
    using Contour.Configurator;

    /// <summary>
    /// The program.
    /// </summary>
    internal class Program
    {
        #region Methods

        /// <summary>
        /// The main.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        private static void Main(string[] args)
        {
            LogManager.Adapter = new ConsoleOutLoggerFactoryAdapter();

            // super bad resolver
            DependencyResolverFunc dependencyResolver = (name, type) => new SomeHandler();

            var configurator = new AppConfigConfigurator(dependencyResolver);
            var busFactory = new BusFactory();

            IBus consumer = busFactory.Create(cfg => configurator.Configure("Consumer", cfg));
            IBus producer = busFactory.Create(cfg => configurator.Configure("Producer", cfg));

            using (new Timer(_ => producer.Emit(":msg", new { }), null, 0, 1000))
            {
                Console.ReadKey(false);
            }

            producer.Dispose();
            consumer.Dispose();
        }

        #endregion
    }
}
