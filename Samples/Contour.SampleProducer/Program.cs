namespace Contour.SampleProducer
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Common.Logging;
    using Common.Logging.Simple;

    using Contour.Configuration;
    using Contour.Sending;
    using Contour.Transport.RabbitMQ;

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
        /// The bus.
        /// </summary>
        private static IBus Bus;

        /// <summary>
        /// The message number.
        /// </summary>
        private static long MessageNumber;

        /// <summary>
        /// The params.
        /// </summary>
        private static ProducerParams Params;

        #endregion

        #region Methods

        /// <summary>
        /// The configure bus.
        /// </summary>
        /// <param name="cfg">
        /// The cfg.
        /// </param>
        private static void ConfigureBus(IBusConfigurator cfg)
        {
            cfg.UseRabbitMq();
            cfg.SetEndpoint(Params.Endpoint);
            cfg.SetConnectionString(Params.ConnectionString);
            cfg.UseParallelismLevel(Params.ParallelismLevel);

            ISenderConfigurator routeCfg = cfg.Route(Params.Type).
                WithDefaultCallbackEndpoint().
                Persistently();
            if (Params.RequireConfirmation)
            {
                routeCfg.WithConfirmation();
            }

            routeCfg = cfg.Route("bad.msg").
                WithDefaultCallbackEndpoint();
            if (Params.RequireConfirmation)
            {
                routeCfg.WithConfirmation();
            }
        }

        /// <summary>
        /// The create emitter.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="cancellationToken">
        /// The cancellation token.
        /// </param>
        /// <returns>
        /// The <see cref="Action"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// </exception>
        private static Action CreateEmitter(int id, CancellationToken cancellationToken)
        {
            return () =>
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        long messageNumber = Interlocked.Increment(ref MessageNumber);
                        try
                        {
                            switch (Params.Type)
                            {
                                case ProducerParams.OneWay:

                                    string label = Params.Type;
                                    Message message = new OneWay(messageNumber);

                                    if (Params.Misbehave)
                                    {
                                        if (Random.Next(10) == 0)
                                        {
                                            label = "bad.msg";
                                        }

                                        if (Random.Next(10) == 0)
                                        {
                                            message = new BadOneWay(messageNumber);
                                        }
                                    }

                                    Console.WriteLine("#{0}: Emitting {1} with label {2}...", id, messageNumber, label);
                                    Bus.Emit(label, message).
                                        ContinueWith(r => Console.WriteLine("#{0}: Received {1} confirmation for {2}.", id, !r.IsFaulted ? "successful" : "failure", messageNumber), cancellationToken);
                                    break;
                                case ProducerParams.Request:

                                    label = Params.Type;
                                    message = new Request(messageNumber);

                                    if (Params.Misbehave)
                                    {
                                        if (Random.Next(10) == 0)
                                        {
                                            label = "bad.msg";
                                        }

                                        if (Random.Next(10) == 0)
                                        {
                                            message = new BadOneWay(messageNumber);
                                        }
                                    }

                                    Console.WriteLine("#{0}: Requesting {1} using label {2}...", id, messageNumber, label);
                                    Bus.RequestAsync<Message, Response>(label, message, new RequestOptions { Timeout = TimeSpan.FromSeconds(5) }).
                                        ContinueWith(
                                            r =>
                                                {
                                                    if (r.IsFaulted)
                                                    {
                                                        Console.WriteLine("#{0}: Timeout while waiting for {1}.", id, messageNumber);
                                                    }
                                                    else if (r.IsCompleted)
                                                    {
                                                        Console.WriteLine("#{0}: Received response {1}.", id, r.Result.Number);
                                                    }
                                                }, 
                                            cancellationToken);
                                    break;
                                default:
                                    throw new NotSupportedException("Unknown producer type: " + Params.Type);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("#{0}: Caught exception: {1}.", id, ex);
                            Console.ForegroundColor = ConsoleColor.White;
                        }

                        cancellationToken.WaitHandle.WaitOne(Params.Delay);
                    }
                };
        }

        /// <summary>
        /// The main.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        private static void Main(string[] args)
        {
            LogManager.Adapter = new ConsoleOutLoggerFactoryAdapter();

            Params = ProducerParams.ReadFromAppSettings();

            Console.WriteLine("Starting producer [{0}]...", Params.Endpoint);
            using (Bus = new BusFactory().Create(ConfigureBus))
            {
                var cancellationTokenSource = new CancellationTokenSource();
                CancellationToken token = cancellationTokenSource.Token;

                Task[] emittingTasks = Enumerable.Range(0, Params.EmittingThreads).
                    Select(i => Task.Factory.StartNew(CreateEmitter(i, token), token)).
                    ToArray();

                Console.ReadKey(true);
                Console.WriteLine("Shutting down producer [{0}]...", Params.Endpoint);

                cancellationTokenSource.Cancel();
                Task.WaitAll(emittingTasks);
            }
        }

        #endregion
    }
}
