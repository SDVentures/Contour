namespace Contour.SampleConsumer
{
    using System;
    using System.Threading;

    using Common.Logging;
    using Common.Logging.Simple;

    using Contour.Configuration;
    using Contour.Receiving;
    using Contour.Transport.RabbitMQ;

    using Timer = System.Timers.Timer;

    /// <summary>
    /// The program.
    /// </summary>
    internal class Program
    {
        #region Static Fields

        /// <summary>
        /// The random.
        /// </summary>
        private static readonly Random Random = new Random();

        /// <summary>
        /// The params.
        /// </summary>
        private static ConsumerParams Params;

        #endregion

        #region Methods

        /// <summary>
        /// The configure bus.
        /// </summary>
        /// <param name="cfg">
        /// The cfg.
        /// </param>
        /// <exception cref="AccessViolationException">
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// </exception>
        private static void ConfigureBus(IBusConfigurator cfg)
        {
            cfg.UseRabbitMq();
            cfg.SetEndpoint(Params.Endpoint);
            cfg.SetConnectionString(Params.ConnectionString);
            cfg.UseParallelismLevel(Params.ParallelismLevel);

            IReceiverConfigurator subscriberCfg = cfg.On(Params.Type);
            if (Params.RequireAccept)
            {
                subscriberCfg.RequiresAccept();
            }

            switch (Params.Type)
            {
                case ConsumerParams.OneWay:
                    subscriberCfg.ReactWith<OneWay>(
                        (m, ctx) =>
                            {
                                Console.WriteLine("Received {0} with label [{1}].", m.Number, ctx.Message.Label);

                                Thread.Sleep((int)Params.ConsumingDelay);

                                if (Params.Misbehave)
                                {
                                    if (Random.Next(10) == 0)
                                    {
                                        throw new AccessViolationException("un oh...");
                                    }
                                }

                                ctx.Accept();
                            });
                    break;
                case ConsumerParams.Request:

                    /*
					subscriberCfg.ReactWith<WrongRequest>((m, ctx) =>
					{
						Console.WriteLine("Received {0}. Sending response...", m.Number);
						ctx.Reply(new Response(666));
						ctx.Accept();
					});
*/
                    subscriberCfg.ReactWith<Request>(
                        (m, ctx) =>
                            {
                                Console.WriteLine("Received {0}. Sending response...", m.Number);

                                if (Params.Misbehave)
                                {
                                    if (Random.Next(10) == 0)
                                    {
                                        throw new AccessViolationException("un oh...");
                                    }
                                }

                                // Thread.Sleep(50000);
                                ctx.Reply(new Response(m.Number));
                                ctx.Accept();
                            });
                    break;
                default:
                    throw new NotSupportedException("Unknown consumer type: " + Params.Type);
            }
        }

        /// <summary>
        /// The main.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        // ReSharper disable once UnusedParameter.Local
        private static void Main(string[] args)
        {
            LogManager.Adapter = new ConsoleOutLoggerFactoryAdapter();

            Params = ConsumerParams.ReadFromAppSettings();

            Console.WriteLine("Starting consumer [{0}]...", Params.Endpoint);
            using (IBus bus = new BusFactory().Create(ConfigureBus, false))
            {
                bus.Start(false);

                var t = new Timer(10000);
                // ReSharper disable once AccessToDisposedClosure
                t.Elapsed += (sender, eventArgs) => ((IBusAdvanced)bus).Panic();

                if (Params.Misbehave)
                {
                    t.Start();
                }

                Console.ReadKey(true);

                t.Close();
                Console.WriteLine("Shutting down consumer [{0}]...", Params.Endpoint);
            }
        }

        #endregion
    }
}
