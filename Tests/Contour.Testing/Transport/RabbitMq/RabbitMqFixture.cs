using System;
using System.Collections.Generic;
using System.Configuration;

using Contour.Configuration;
using Contour.Testing.Plumbing;
using Contour.Transport.RabbitMQ;

using NUnit.Framework;

namespace Contour.Testing.Transport.RabbitMq
{
    /// <summary>
    /// Базовый класс для тестовых наборов шины сообщений на транспорте <c>RabbitMQ</c>.
    /// </summary>
    /// <remarks>
    /// Позволяет конфигурировать параметры подключения к брокеру <c>RabbitMQ</c> с помощью параметров в конфигурационной секции <c>AppSettings</c>:
    /// <list type="table">
    /// <item><term><c>managementConnection</c></term><description> строка подключения к <c>HTTP API</c> брокера.</description></item>
    /// <item><term><c>amqpConnection</c></term><description> строка подключения к брокеру.</description></item>
    /// <item><term><c>testUsername</c></term><description> имя пользователя, от имени которого выполняется тест.</description></item>
    /// <item><term><c>testPassword</c></term><description> пароль пользователя, от имени которого выполняется тест.</description></item>
    /// <item><term><c>adminPassword</c></term><description> имя пользователя, от имени которого настраивается брокер.</description></item>
    /// <item><term><c>adminPassword</c></term><description> пароль пользователя, от имени которого настраивается брокер.</description></item>
    /// </list>
    /// </remarks>
    public class RabbitMqFixture
    {
        private Broker broker;

        private string vhostName;

        private IList<IBus> endpoints = new List<IBus>();

        private string managementConnection = "http://rabbitmq-test:15672/";

        private string amqpConnection = "amqp://rabbitmq-test:5672/";

        private string testUsername = "guest";

        private string testPassword = "guest";

        private string adminUsername = "guest";

        private string adminPassword = "guest";

        /// <summary>
        /// <c>RabbitMQ</c> брокер, с которым выполняется тестирование.
        /// </summary>
        protected Broker Broker
        {
            get
            {
                return this.broker;
            }
        }

        /// <summary>
        /// <c>VHost</c> брокера, с которым выполняется тестирование.
        /// </summary>
        protected string VhostName
        {
            get
            {
                return this.vhostName;
            }
        }

        protected string AmqpConnection 
        {
            get
            {
                return this.amqpConnection;
            }
        }

        /// <summary>
        /// Настраивает окружение перед работой теста.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            this.PreSetUp();

            this.endpoints = new List<IBus>();
            this.broker = new Broker(this.managementConnection, this.adminUsername, this.adminPassword);
            this.vhostName = "test" + Guid.NewGuid().ToString("n");
            this.broker.CreateHost(this.vhostName);
            this.broker.CreateUser(this.vhostName, this.testUsername, this.testPassword);
            this.broker.SetPermissions(this.vhostName, this.testUsername);
        }

        /// <summary>
        /// Очищает окружение после работы теста.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            foreach (var bus in this.endpoints)
            {
                bus.Dispose();
            }

            this.endpoints.Clear();
            this.broker.DeleteHost(this.vhostName);
            this.vhostName = null;
            this.broker = null;
        }

        /// <summary>
        /// Создает конечную точку шины.
        /// </summary>
        /// <param name="createBus">Делегат создания конечной точки.</param>
        /// <returns>Конечная точка шины.</returns>
        protected IBus CreateBus(Func<IBus> createBus)
        {
            IBus bus = createBus();
            this.endpoints.Add(bus);

            return bus;
        }

        /// <summary>
        /// Создает и конфигурирует конечную точку шины сообщений.
        /// Конечная точка автоматически удаляется после выполнения теста.
        /// </summary>
        /// <param name="name">Имя конечной точки.</param>
        /// <param name="configureAction">Указатель на метод конфигурирования конечной точки.</param>
        /// <param name="shouldStart"><c>true</c> - если конечная точка должна запуститься сразу после создания.</param>
        /// <returns>Созданная конечная точка.</returns>
        protected IBus ConfigureBus(string name, Action<IBusConfigurator> configureAction, bool shouldStart = false)
        {
            IBus bus = this.CreateBus(
                () => new BusFactory().Create(
                c =>
                    {
                        c.UseRabbitMq();
                        c.SetEndpoint(name);
                        c.SetConnectionString(this.amqpConnection + this.vhostName);

                        configureAction(c);
                    },
                    shouldStart));
            return bus;
        }

        /// <summary>
        /// Создает и запускает конечную точку.
        /// Конечная точка автоматически удаляется после выполнения теста.
        /// </summary>
        /// <param name="name">Имя конечной точки.</param>
        /// <param name="configureAction">Указатель на метод конфигурирования конечной точки.</param>
        /// <returns>Созданная конечная точка.</returns>
        protected IBus StartBus(string name, Action<IBusConfigurator> configureAction)
        {
            return this.ConfigureBus(name, configureAction, true);
        }

        /// <summary>
        /// Инициализация конфигурационных настроек.
        /// </summary>
        protected virtual void PreSetUp()
        {
            this.managementConnection = ConfigurationManager.AppSettings["managementConnection"] ?? this.managementConnection;
            this.amqpConnection = ConfigurationManager.AppSettings["amqpConnection"] ?? this.amqpConnection;
            this.testUsername = ConfigurationManager.AppSettings["testUsername"] ?? this.testUsername;
            this.testPassword = ConfigurationManager.AppSettings["testPassword"] ?? this.testPassword;
            this.adminUsername = ConfigurationManager.AppSettings["adminUsername"] ?? this.adminUsername;
            this.adminPassword = ConfigurationManager.AppSettings["adminPassword"] ?? this.adminPassword;
        }
    }
}
