using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

using Contour.Testing.Transport.RabbitMq;

using NUnit.Framework;

namespace Contour.RabbitMq.Tests
{
    /// <summary>
    /// Набор тестов для проверки обработки заголовков сообщения.
    /// </summary>
    public class MessageHeaderSpecs
    {
        /// <summary>
        /// При обработке входящего сообщения и публикации нового.
        /// </summary>
        [TestFixture]
        [Category("Integration")]
        public class WhenConsumingAndProduceMessage : RabbitMqFixture
        {
            /// <summary>
            /// Должны копироваться заголовки инициирующего сообщения.
            /// </summary>
            [Test]
            public void ShouldCopiesHeadersOfInitialMessage()
            {
                IMessage message = null;
                var waitHandle = new AutoResetEvent(false);

                IBus producer = this.StartBus(
                    "producer",
                    cfg => cfg.Route("command.handle.this"));

                this.StartBus(
                    "consumer",
                    cfg =>
                        {
                            cfg.Route("event.this.handled");
                            cfg.On("command.handle.this")
                                .ReactWith<ExpandoObject>(
                                    (m, ctx) => ctx.Bus.Emit("event.this.handled", new { }));
                        });

                this.StartBus(
                    "trap",
                    cfg => cfg.On("event.this.handled")
                               .ReactWith<ExpandoObject>(
                                   (m, ctx) =>
                                       {
                                           message = ctx.Message;
                                           waitHandle.Set();
                                       }));

                var utc = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);

                var producerTime = "x-producer-time";

                producer.Emit(
                    "command.handle.this".ToMessageLabel(),
                    new { },
                    new Dictionary<string, object> { { producerTime, utc } });

                Assert.IsTrue(waitHandle.WaitOne(), "Тест должен завершиться за указанное время.");
                Assert.IsTrue(message.Headers.ContainsKey(producerTime), "Должен быть установлен заголовок переданный первоисточником.");
                Assert.AreEqual(utc, Headers.GetString(message.Headers, producerTime), "Заголовок должен содержать корректные данные.");
            }

            /// <summary>
            /// Должны копироваться заголовки при отправке сообщений в новом потоке.
            /// </summary>
            [Test]
            public void ShouldCopiesHeadersForNewTask()
            {
                IMessage message = null;
                var waitHandle = new AutoResetEvent(false);

                IBus producer = this.StartBus(
                    "producer",
                    cfg => cfg.Route("command.handle.this"));

                this.StartBus(
                    "consumer",
                    cfg =>
                        {
                            cfg.Route("event.this.handled");
                            cfg.On("command.handle.this")
                                .ReactWith<ExpandoObject>(
                                    (m, ctx) => Task.Factory.StartNew(
                                        () =>
                                            {
                                                Thread.Sleep(100);
                                                ctx.Bus.Emit("event.this.handled", new { });
                                            },
                                        TaskCreationOptions.LongRunning));
                        });

                this.StartBus(
                    "trap",
                    cfg => cfg.On("event.this.handled")
                               .ReactWith<ExpandoObject>(
                                   (m, ctx) =>
                                       {
                                           message = ctx.Message;
                                           waitHandle.Set();
                                       }));

                var utc = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);

                var producerTime = "x-producer-time";

                producer.Emit(
                    "command.handle.this".ToMessageLabel(),
                    new { },
                    new Dictionary<string, object> { { producerTime, utc } });

                Assert.IsTrue(waitHandle.WaitOne(), "Тест должен завершиться за указанное время.");
                Assert.IsTrue(message.Headers.ContainsKey(producerTime), "Должен быть установлен заголовок переданный первоисточником.");
                Assert.AreEqual(utc, Headers.GetString(message.Headers, producerTime), "Заголовок должен содержать корректные данные.");
            }

            /// <summary>
            /// Должны копироваться заголовке при использовании задачи из пула потоков.
            /// </summary>
            [Test]
            public void ShouldCopiesHeadersForQueueUserItem()
            {
                IMessage message = null;
                var waitHandle = new AutoResetEvent(false);

                IBus producer = this.StartBus(
                    "producer",
                    cfg => cfg.Route("command.handle.this"));

                this.StartBus(
                    "consumer",
                    cfg =>
                        {
                            cfg.Route("event.this.handled");
                            cfg.On("command.handle.this")
                                .ReactWith<ExpandoObject>(
                                    (m, ctx) => ThreadPool.QueueUserWorkItem(
                                        state =>
                                            {
                                                Thread.Sleep(100);
                                                ctx.Bus.Emit("event.this.handled", new { });
                                            }));
                        });

                this.StartBus(
                    "trap",
                    cfg => cfg.On("event.this.handled")
                               .ReactWith<ExpandoObject>(
                                   (m, ctx) =>
                                       {
                                           message = ctx.Message;
                                           waitHandle.Set();
                                       }));

                var utc = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);

                var producerTime = "x-producer-time";

                producer.Emit(
                    "command.handle.this".ToMessageLabel(),
                    new { },
                    new Dictionary<string, object> { { producerTime, utc } });

                Assert.IsTrue(waitHandle.WaitOne(), "Тест должен завершиться за указанное время.");
                Assert.IsTrue(message.Headers.ContainsKey(producerTime), "Должен быть установлен заголовок переданный первоисточником.");
                Assert.AreEqual(utc, Headers.GetString(message.Headers, producerTime), "Заголовок должен содержать корректные данные.");
            }

            /// <summary>
            /// Должны копироваться заголовки запроса.
            /// </summary>
            [Test]
            public void ShouldCopiesHeadersOfInitialRequestMessage()
            {
                IMessage message = null;
                var waitHandle = new AutoResetEvent(false);

                IBus producer = this.StartBus(
                    "producer",
                    cfg => cfg.Route("command.handle.this").WithDefaultCallbackEndpoint());

                this.StartBus(
                    "consumer",
                    cfg =>
                    {
                        cfg.Route("event.this.handled");
                        cfg.On("command.handle.this").ReactWith<ExpandoObject>(
                            (m, ctx) =>
                                {
                                    ctx.Bus.Emit("event.this.handled", new { }); 
                                    ctx.Reply(new { });
                                });
                    });

                this.StartBus(
                    "trap",
                    cfg => cfg.On("event.this.handled").ReactWith<ExpandoObject>(
                        (m, ctx) =>
                        {
                            message = ctx.Message;
                            waitHandle.Set();
                        }));

                var utc = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);

                var producerTime = "x-producer-time";

                producer.Request<object, object>(
                    "command.handle.this".ToMessageLabel(), 
                    new { },
                    new Dictionary<string, object> { { producerTime, utc } }, 
                    o => { });

                Assert.IsTrue(waitHandle.WaitOne(), "Тест должен завершиться за указанное время.");
                Assert.IsTrue(message.Headers.ContainsKey(producerTime), "Должен быть установлен заголовок переданный первоисточником.");
                Assert.AreEqual(utc, Headers.GetString(message.Headers, producerTime), "Заголовок должен содержать корректные данные.");
            }

            /// <summary>
            /// Должны копироваться заголовки асинхронного запроса.
            /// </summary>
            [Test]
            public void ShouldCopiesHeadersOfInitialAsyncRequestMessage()
            {
                IMessage message = null;
                var waitHandle = new AutoResetEvent(false);

                IBus producer = this.StartBus(
                    "producer",
                    cfg => cfg.Route("command.handle.this").WithDefaultCallbackEndpoint());

                this.StartBus(
                    "consumer",
                    cfg =>
                    {
                        cfg.Route("event.this.handled");
                        cfg.On("command.handle.this").ReactWith<ExpandoObject>(
                            (m, ctx) =>
                            {
                                ctx.Bus.Emit("event.this.handled", new { });
                                ctx.Reply(new { });
                            });
                    });

                this.StartBus(
                    "trap",
                    cfg => cfg.On("event.this.handled").ReactWith<ExpandoObject>(
                        (m, ctx) =>
                        {
                            message = ctx.Message;
                            waitHandle.Set();
                        }));

                var utc = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);

                var producerTime = "x-producer-time";

                Assert.IsTrue(
                    producer
                        .RequestAsync<object, object>(
                            "command.handle.this".ToMessageLabel(),
                            new { },
                            new Dictionary<string, object> { { producerTime, utc } }).ContinueWith(t => { })
                        .Wait(TimeSpan.FromSeconds(5)),
                    "Операция запроса должна получить ответ.");

                Assert.IsTrue(waitHandle.WaitOne(), "Тест должен завершиться за указанное время.");
                Assert.IsTrue(message.Headers.ContainsKey(producerTime), "Должен быть установлен заголовок переданный первоисточником.");
                Assert.AreEqual(utc, Headers.GetString(message.Headers, producerTime), "Заголовок должен содержать корректные данные.");
            }

            /// <summary>
            /// Должен формироваться путь прохождения сообщения.
            /// </summary>
            [Test]
            public void ShouldFillBreadCrumbs()
            {
                IMessage message = null;
                var waitHandle = new AutoResetEvent(false);

                const string ProducerName = "producer";
                IBus producer = this.StartBus(
                    ProducerName,
                    cfg => cfg.Route("command.handle.this").WithDefaultCallbackEndpoint());

                const string ConsumerName = "consumer";
                this.StartBus(
                    ConsumerName,
                    cfg =>
                    {
                        cfg.Route("event.this.handled");
                        cfg.On("command.handle.this").ReactWith<ExpandoObject>(
                            (m, ctx) =>
                            {
                                ctx.Bus.Emit("event.this.handled", new { });
                                ctx.Reply(new { });
                            });
                    });

                const string TrapName = "trap";
                this.StartBus(
                    TrapName,
                    cfg => cfg.On("event.this.handled").ReactWith<ExpandoObject>(
                        (m, ctx) =>
                        {
                            message = ctx.Message;
                            waitHandle.Set();
                        }));

                Assert.IsTrue(
                    producer
                        .RequestAsync<object, object>(
                            "command.handle.this".ToMessageLabel(),
                            new { })
                        .ContinueWith(t => { })
                        .Wait(TimeSpan.FromSeconds(5)),
                    "Операция запроса должна получить ответ.");

                Assert.IsTrue(waitHandle.WaitOne(), "Тест должен завершиться за указанное время.");
                Assert.IsTrue(message.Headers.ContainsKey(Headers.Breadcrumbs), "Должен быть установлен заголовок пути сообщения.");
                Assert.AreEqual(
                    string.Format("{0};{1}", ProducerName, ConsumerName), 
                    Headers.GetString(message.Headers, Headers.Breadcrumbs), 
                    "Заголовок должен содержать корректные данные.");
            }

            /// <summary>
            /// Должен формироваться путь прохождения сообщения.
            /// </summary>
            [Test]
            public void ShouldNotMultyFillBreadCrumbsWhenMassEmit()
            {
                IMessage message = null;
                var waitHandle = new CountdownEvent(10);

                const string ProducerName = "producer";
                IBus producer = this.StartBus(
                    ProducerName,
                    cfg => cfg.Route("command.handle.this").WithDefaultCallbackEndpoint());

                const string ConsumerName = "consumer";
                this.StartBus(
                    ConsumerName,
                    cfg =>
                    {
                        cfg.Route("event.this.handled");
                        cfg.On("command.handle.this").ReactWith<ExpandoObject>(
                            (m, ctx) =>
                            {
                                for (var i = 0; i < 10; i++)
                                {
                                    ctx.Bus.Emit("event.this.handled", new { });
                                }

                                ctx.Reply(new { });
                            });
                    });

                const string TrapName = "trap";
                this.StartBus(
                    TrapName,
                    cfg => cfg.On("event.this.handled").ReactWith<ExpandoObject>(
                        (m, ctx) =>
                        {
                            message = ctx.Message;
                            waitHandle.Signal();
                        }));

                Assert.IsTrue(
                    producer
                        .RequestAsync<object, object>(
                            "command.handle.this".ToMessageLabel(),
                            new { })
                        .ContinueWith(t => { })
                        .Wait(TimeSpan.FromSeconds(5)),
                    "Операция запроса должна получить ответ.");

                Assert.IsTrue(waitHandle.Wait(TimeSpan.FromSeconds(5)), "Тест должен завершиться за указанное время.");
                Assert.IsTrue(message.Headers.ContainsKey(Headers.Breadcrumbs), "Должен быть установлен заголовок пути сообщения.");
                Assert.AreEqual(
                    string.Format("{0};{1}", ProducerName, ConsumerName),
                    Headers.GetString(message.Headers, Headers.Breadcrumbs),
                    "Заголовок должен содержать корректные данные.");
            }

            /// <summary>
            /// Должен передаваться сквозной идентификатор сообщения.
            /// </summary>
            [Test]
            public void ShouldFillOriginalMessageId()
            {
                IMessage message = null;
                var waitHandle = new AutoResetEvent(false);

                const string ProducerName = "producer";
                IBus producer = this.StartBus(
                    ProducerName,
                    cfg => cfg.Route("command.handle.this").WithDefaultCallbackEndpoint());

                const string ConsumerName = "consumer";
                this.StartBus(
                    ConsumerName,
                    cfg =>
                    {
                        cfg.Route("event.this.handled");
                        cfg.On("command.handle.this").ReactWith<ExpandoObject>(
                            (m, ctx) =>
                            {
                                ctx.Bus.Emit("event.this.handled", new { });
                                ctx.Reply(new { });
                            });
                    });

                const string TrapName = "trap";
                this.StartBus(
                    TrapName,
                    cfg => cfg.On("event.this.handled").ReactWith<ExpandoObject>(
                        (m, ctx) =>
                        {
                            message = ctx.Message;
                            waitHandle.Set();
                        }));

                Assert.IsTrue(
                    producer
                        .RequestAsync<object, object>(
                            "command.handle.this".ToMessageLabel(),
                            new { })
                        .ContinueWith(t => { })
                        .Wait(TimeSpan.FromSeconds(5)),
                    "Операция запроса должна получить ответ.");

                Assert.IsTrue(waitHandle.WaitOne(), "Тест должен завершиться за указанное время.");
                Assert.IsTrue(message.Headers.ContainsKey(Headers.OriginalMessageId), "Должен быть установлен заголовок с индентификатором первого сообщения.");
            }

            /// <summary>
            /// Сквозной идентификатор сообщения не должен меняться.
            /// </summary>
            [Test]
            public void ShouldNotChangedOriginalMessageId()
            {
                IMessage message = null;
                var waitHandle = new AutoResetEvent(false);

                const string ProducerName = "producer";
                IBus producer = this.StartBus(
                    ProducerName,
                    cfg => cfg.Route("command.handle.this").WithDefaultCallbackEndpoint());

                const string ConsumerName = "consumer";
                string initalEndToEndId = string.Empty;

                this.StartBus(
                    ConsumerName,
                    cfg =>
                    {
                        cfg.Route("event.this.handled");
                        cfg.On("command.handle.this").ReactWith<ExpandoObject>(
                            (m, ctx) =>
                            {
                                initalEndToEndId = Headers.GetString(ctx.Message.Headers, Headers.OriginalMessageId);
                                ctx.Bus.Emit("event.this.handled", new { });
                                ctx.Reply(new { });
                            });
                    });

                const string TrapName = "trap";
                this.StartBus(
                    TrapName,
                    cfg => cfg.On("event.this.handled").ReactWith<ExpandoObject>(
                        (m, ctx) =>
                        {
                            message = ctx.Message;
                            waitHandle.Set();
                        }));

                Assert.IsTrue(
                    producer
                        .RequestAsync<object, object>(
                            "command.handle.this".ToMessageLabel(),
                            new { })
                        .ContinueWith(t => { })
                        .Wait(TimeSpan.FromSeconds(5)),
                    "Операция запроса должна получить ответ.");

                Assert.IsTrue(waitHandle.WaitOne(), "Тест должен завершиться за указанное время.");
                Assert.AreEqual(initalEndToEndId, Headers.GetString(message.Headers, Headers.OriginalMessageId), "Заголовок со идентификатором первого сообщения.");
            }
        }
    }
}
