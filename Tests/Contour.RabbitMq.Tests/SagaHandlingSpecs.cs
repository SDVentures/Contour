using System;
using System.Collections.Generic;
using System.Threading;

using Contour.Configuration;
using Contour.Configuration.Configuration;
using Contour.Receiving;
using Contour.Receiving.Sagas;
using Contour.Testing.Transport.RabbitMq;

using NUnit.Framework;

namespace Contour.RabbitMq.Tests
{
    /// <summary>
    /// Набор тестов для саги.
    /// </summary>
    public class SagaHandlingSpecs
    {
        /// <summary>
        /// При получении сообщения
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class WhenHandleMessage : RabbitMqFixture
        {
            /// <summary>
            /// Должен обрабатываться шаг саги.
            /// </summary>
            [Test]
            public void ShouldProcessSagaStep()
            {
                var commandProcessdata = "command.processdata";
                var documentResult = "document.result";
                var commandGetdata = "command.getdata";

                var autoResetEvent = new AutoResetEvent(false);

                Action<ISagaContext<List<string>, string>, IConsumingContext<string>> finalAction = (sc, cc) =>
                {
                    sc.Data.Add("coordinator");
                    sc.Data.Add("response");
                    autoResetEvent.Set();
                    sc.Complete();
                };

                Action<ISagaContext<List<string>, string>, IConsumingContext<string>> initialAction = (sc, cc) =>
                {
                    if (sc.Data == null)
                    {
                        sc.UpdateData(new List<string>());
                    }

                    sc.Data.Add("coordinator");
                    cc.Bus.Emit(
                        commandGetdata.ToMessageLabel(),
                        new { },
                        new Dictionary<string, object> { { Headers.CorrelationId, sc.SagaId } });
                    sc.Data.Add("request");
                };

                var initiator = this.StartBus("initiator", cfg => cfg.Route(commandProcessdata));
                this.StartBus(
                    "coordinator",
                    cfg =>
                        {
                            cfg.AsSagaStep<List<string>, string, string>(commandProcessdata.ToMessageLabel())
                                .ReactWith(true, initialAction)
                                .UseSagaIdSeparator(
                                    m => m.Headers.ContainsKey(Headers.CorrelationId) 
                                        ? Headers.GetString(m.Headers, Headers.CorrelationId) 
                                        : Guid.NewGuid().ToString("N"));

                            cfg.AsSagaStep<List<string>, string, string>(documentResult.ToMessageLabel())
                                .ReactWith(false, finalAction);

                            cfg.Route(commandGetdata);
                        });
                this.StartBus(
                    "mediator",
                    cfg =>
                        {
                            cfg.On(commandGetdata)
                                .ReactWith((object m, IConsumingContext<object> ctx) => ctx.Bus.Emit(documentResult, "finish"));
                            cfg.Route(documentResult);
                        });

                initiator.WhenReady.WaitOne(TimeSpan.FromSeconds(5));
                initiator.Emit(commandProcessdata, "start");
                var result = autoResetEvent.WaitOne(TimeSpan.FromSeconds(5));
                Assert.IsTrue(result, "Сага должна быть обработана.");
            }
        }
    }
}
