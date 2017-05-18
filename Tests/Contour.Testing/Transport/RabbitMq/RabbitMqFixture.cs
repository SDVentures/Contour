using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        /// <summary>
        /// Gets the endpoints.
        /// </summary>
        protected IList<IBus> Endpoints { get; set; } = new List<IBus>();

        /// <summary>
        /// Gets the management connection.
        /// </summary>
        protected string ManagementConnection { get; private set; } = "http://rabbitmq-test:15672/";

        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        protected string ConnectionString { get; set; }

        /// <summary>
        /// Gets the test user name.
        /// </summary>
        protected string TestUserName { get; private set; } = "guest";

        /// <summary>
        /// Gets the test password.
        /// </summary>
        protected string TestUserPassword { get; private set; } = "guest";

        /// <summary>
        /// Gets the admin username.
        /// </summary>
        protected string AdminUserName { get; private set; } = "guest";

        /// <summary>
        /// Gets the admin password.
        /// </summary>
        protected string AdminUserPassword { get; private set; } = "guest";

        /// <summary>
        /// <c>RabbitMQ</c> брокер, с которым выполняется тестирование.
        /// </summary>
        protected Broker Broker { get; set; }

        /// <summary>
        /// <c>VHost</c> брокера, с которым выполняется тестирование.
        /// </summary>
        protected string VhostName { get; private set; }

        /// <summary>
        /// Gets the broker URL.
        /// </summary>
        protected string Url { get; private set; } = "amqp://rabbitmq-test:5672/";

        /// <summary>
        /// Настраивает окружение перед работой теста.
        /// </summary>
        [SetUp]
        public virtual void SetUp()
        {
            this.PreSetUp();

            this.Endpoints = new List<IBus>();
            this.Broker = new Broker(this.ManagementConnection, this.AdminUserName, this.AdminUserPassword);
            this.VhostName = "test" + Guid.NewGuid().ToString("n");

            this.ConnectionString = this.Url + this.VhostName;

            this.Broker.CreateHost(this.VhostName);
            this.Broker.CreateUser(this.VhostName, this.TestUserName, this.TestUserPassword);
            this.Broker.SetPermissions(this.VhostName, this.TestUserName);
        }

        /// <summary>
        /// Очищает окружение после работы теста.
        /// </summary>
        [TearDown]
        public virtual void TearDown()
        {
            foreach (var bus in this.Endpoints)
            {
                bus.Dispose();
            }

            this.Endpoints.Clear();
            this.Broker.DeleteHost(this.VhostName);
            this.VhostName = null;
            this.Broker = null;
        }

        /// <summary>
        /// Создает конечную точку шины.
        /// </summary>
        /// <param name="createBus">Делегат создания конечной точки.</param>
        /// <returns>Конечная точка шины.</returns>
        protected IBus CreateBus(Func<IBus> createBus)
        {
            IBus bus = createBus();
            this.Endpoints.Add(bus);

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
            var bus = this.CreateBus(
                () => new TestFactory().Create(
                    c =>
                    {
                        c.SetConnectionString(this.ConnectionString);
                        c.UseRabbitMq();
                        c.SetEndpoint(name);

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
            this.ManagementConnection = ConfigurationManager.AppSettings["managementConnection"] ??
                                        this.ManagementConnection;
            this.Url = ConfigurationManager.AppSettings["amqpConnection"] ?? this.Url;
            this.TestUserName = ConfigurationManager.AppSettings["testUsername"] ?? this.TestUserName;
            this.TestUserPassword = ConfigurationManager.AppSettings["testPassword"] ?? this.TestUserPassword;
            this.AdminUserName = ConfigurationManager.AppSettings["adminUsername"] ?? this.AdminUserName;
            this.AdminUserPassword = ConfigurationManager.AppSettings["adminPassword"] ?? this.AdminUserPassword;
        }

        /// <summary>
        /// Initializes a list of virtual hosts
        /// </summary>
        /// <returns></returns>
        protected IEnumerable<string> InitializeHosts(IEnumerable<string> hostNames)
        {
            var list = new List<string>();

            foreach (var name in hostNames)
            {
                this.Broker.CreateHost(name);
                this.Broker.CreateUser(name, this.TestUserName, this.TestUserPassword);
                this.Broker.SetPermissions(name, this.TestUserName);

                var url = $"{this.Url}{name}";
                list.Add(url);
            }

            return list;
        }

        protected IEnumerable<string> GetHostNames(int count)
        {
            return Enumerable.Range(0, count).Select(i => "test" + Guid.NewGuid().ToString("n") + $"_{i}");
        }

        /// <summary>
        /// Deletes all virtual hosts except the root
        /// </summary>
        protected void DeleteHosts()
        {
            var hosts = this.Broker.GetHosts().Where(h => h.name != "/");
            foreach (var host in hosts)
            {
                this.Broker.DeleteHost(host.name);
            }
        }
    }
}