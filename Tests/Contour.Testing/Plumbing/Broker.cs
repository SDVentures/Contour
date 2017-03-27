using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Deserializers;

namespace Contour.Testing.Plumbing
{
    /// <summary>
    /// Обеспечивает подключение к брокеру <c>RabbitMQ</c>.
    /// </summary>
    public class Broker
    {
        private readonly string connection;

        private readonly string username;

        private readonly string password;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Broker"/>. 
        /// </summary>
        /// <param name="connection">Строка подключения к брокеру.</param>
        /// <param name="username">Имя пользователя.</param>
        /// <param name="password">Пароль пользователя.</param>
        public Broker(string connection, string username, string password)
        {
            this.connection = connection;
            this.username = username;
            this.password = password;
        }

        /// <summary>
        /// Создает привязку точки маршрутизации и очереди.
        /// </summary>
        /// <param name="vhostName">Виртуальный хост брокера.</param>
        /// <param name="exchangeName">Имя точки маршрутизации.</param>
        /// <param name="queueName">Имя очереди.</param>
        public void CreateBind(string vhostName, string exchangeName, string queueName)
        {
            var client = this.CreateClient();
            var request = CreateRequest("/api/bindings/{vhost}/e/{exchange}/q/{queue}", Method.POST);
            request.AddUrlSegment("vhost", vhostName);
            request.AddUrlSegment("exchange", exchangeName);
            request.AddUrlSegment("queue", queueName);
            client.Execute(request);
        }

        /// <summary>
        /// Возвращает список сообщений из очереди.
        /// </summary>
        /// <param name="vhostName">Виртуальный хост брокера.</param>
        /// <param name="queueName">Имя очереди.</param>
        /// <param name="count">Получаемое количество сообщений из очереди.</param>
        /// <param name="requeue">Если <c>true</c> - тогда нужно возвращать сообщение в очередь.</param>
        /// <param name="encoding">Кодировка сообщений (по умолчанию <c>auto</c>).</param>
        /// <returns>Список сообщений из очереди.</returns>
        public List<Message> GetMessages(string vhostName, string queueName, int count, bool requeue, string encoding = "auto")
        {
            var options = new
                              {
                                  vhost = vhostName,
                                  name = queueName,
                                  count,
                                  requeue,
                                  encoding
                              };
            var client = this.CreateClient();
            var request = CreateRequest("/api/queues/{vhost}/{queue}/get", Method.POST);
            request.AddUrlSegment("vhost", vhostName);
            request.AddUrlSegment("queue", queueName);
            request.AddBody(options);
            var response = client.Execute<List<Message>>(request);
            return response.StatusCode == HttpStatusCode.OK ? response.Data : new List<Message>();
        }

        /// <summary>
        /// Возвращает список очередей в виртуальном хосте.
        /// </summary>
        /// <param name="vhostName">Виртуальный хост брокера.</param>
        /// <returns>Список очередей виртуального хоста брокера..</returns>
        public IList<Queue> GetQueues(string vhostName)
        {
            var client = this.CreateClient();
            var request = CreateRequest("/api/queues/{name}", Method.GET);
            request.AddUrlSegment("name", vhostName);
            var response = client.Execute<List<Queue>>(request);

            return response.StatusCode == HttpStatusCode.OK ? response.Data : new List<Queue>();
        }

        /// <summary>
        /// Проверяет наличие точки маршрутизации в виртуальном хосте брокера.
        /// </summary>
        /// <param name="exchangeName">Имя точки маршрутизации брокера.</param>
        /// <param name="vhostName">Виртуальный хост брокера.</param>
        /// <returns><c>true</c> - если точка маршрутизации найдена.</returns>
        public bool HasExchange(string exchangeName, string vhostName)
        {
            var client = this.CreateClient();
            var request = CreateRequest("/api/exchanges/{name}", Method.GET);
            request.AddUrlSegment("name", vhostName);
            var response = client.Execute<List<Exchange>>(request);

            return response.StatusCode == HttpStatusCode.OK && response.Data.Any(ex => ex.Name == exchangeName);
        }

        /// <summary>
        /// Проверяет наличие очереди в виртуальном хосте брокера.
        /// </summary>
        /// <param name="queueName">Имя очереди.</param>
        /// <param name="vhostName">Виртуальный хост брокера.</param>
        /// <returns><c>true</c> - если очередь найдена.</returns>
        public bool HasQueue(string queueName, string vhostName)
        {
            var client = this.CreateClient();
            var request = CreateRequest("/api/queues/{name}", Method.GET);
            request.AddUrlSegment("name", vhostName);
            var response = client.Execute<List<Queue>>(request);

            return response.StatusCode == HttpStatusCode.OK && response.Data.Any(queue => queue.Name == queueName);
        }

        /// <summary>
        /// Создает виртуальный хост в брокере.
        /// </summary>
        /// <param name="vhostName">Имя виртуального хоста.</param>
        public void CreateHost(string vhostName)
        {
            var client = this.CreateClient();
            var request = CreateRequest("/api/vhosts/{name}", Method.PUT);
            request.AddUrlSegment("name", vhostName);
            var response = client.Execute(request);
            if (response.StatusCode != HttpStatusCode.Created)
            {
                throw new Exception($"Failed to create a host {vhostName}");
            }
        }

        /// <summary>
        /// Создает очередь в виртуальном хосте брокера.
        /// </summary>
        /// <param name="vhostName">Виртуальный хост брокера.</param>
        /// <param name="queueName">Имя создаваемой очереди.</param>
        public void CreateQueue(string vhostName, string queueName)
        {
            var client = this.CreateClient();
            var request = CreateRequest("/api/queues/{vhost}/{queue}", Method.PUT);
            request.AddUrlSegment("vhost", vhostName);
            request.AddUrlSegment("queue", queueName);
            request.AddHeader("Accept", string.Empty);
            client.Execute(request);
        }

        /// <summary>
        /// Создает пользователя в виртуальном хосте брокера.
        /// </summary>
        /// <param name="vhostName">Виртуальный хост брокера.</param>
        /// <param name="name">Имя пользователя.</param>
        /// <param name="userPassword">Пароль пользователя.</param>
        public void CreateUser(string vhostName, string name, string userPassword)
        {
            var user = this.GetUser(name);
            if (user != null)
            {
                return;
            }

            var client = this.CreateClient();
            var request = CreateRequest("/api/users/{name}", Method.PUT);
            request.AddUrlSegment("name", name);
            request.AddBody(new { password = userPassword, tags = "administrator" });
            client.Execute(request);
        }

        public User GetUser(string name)
        {
            var client = this.CreateClient();
            var request = CreateRequest("/api/users/{name}", Method.GET);
            request.AddUrlSegment("name", name);

            var response = client.Execute<User>(request);
            return response.StatusCode == HttpStatusCode.OK ? response.Data : null;
        }

        public void DeleteUser(string name)
        {
            var client = this.CreateClient();
            var request = CreateRequest("/api/users/{name}", Method.DELETE);
            request.AddUrlSegment("name", name);
            client.Execute(request);
        }

        /// <summary>
        /// Устанавливает права пользователя.
        /// </summary>
        /// <param name="vhostName">Виртуальный хост брокера.</param>
        /// <param name="user">Имя пользователя.</param>
        public void SetPermissions(string vhostName, string user)
        {
            var client = this.CreateClient();
            var request = CreateRequest("/api/permissions/{vhost}/{user}", Method.PUT);
            
            request.AddUrlSegment("vhost", vhostName);
            request.AddUrlSegment("user", user);
            request.AddBody(new { Configure = ".*", Write = ".*", Read = ".*" });
            client.Execute(request);
        }

        /// <summary>
        /// Удаляет виртуальный хост брокера.
        /// </summary>
        /// <param name="vhostName">Имя виртуального хоста брокера.</param>
        public void DeleteHost(string vhostName)
        {
            var client = this.CreateClient();
            var request = CreateRequest("/api/vhosts/{name}", Method.DELETE);
            request.AddUrlSegment("name", vhostName);
            client.Execute(request);
        }

        public void PurgeQueue(string vhostName, string queueName)
        {
            var client = this.CreateClient();
            var request = CreateRequest("/api/queues/{vhost}/{name}/contents", Method.DELETE);
            request.AddUrlSegment("vhost", vhostName);
            request.AddUrlSegment("name", queueName);
            client.Execute(request);
        }

        private static RestRequest CreateRequest(string resource, Method method)
        {
            var request = new RestRequest(resource, method)
                              {
                                  JsonSerializer = new RabbitJsonSerializer(), RequestFormat = DataFormat.Json
                              };
            request.AddHeader("Accept", string.Empty);
            return request;
        }

        private IRestClient CreateClient()
        {
            var client = new RestClient(this.connection)
                             {
                                 Authenticator = new HttpBasicAuthenticator(this.username, this.password)
                             };
            client.AddDefaultHeader("Content-Type", "application/json; charset=utf-8");
            return client;
        }

        public IEnumerable<Connection> GetConnections()
        {
            var client = this.CreateClient();
            var connectionsRequest = CreateRequest("/api/connections", Method.GET);

            var response = client.Execute<List<Connection>>(connectionsRequest);

            if (response.ErrorException != null)
            {
                throw new Exception("Failed to get connections", response.ErrorException);
            }

            var connections = response.Data ?? new List<Connection>();
            return connections;
        }

        public void DropConnections()
        {
            var connections = this.GetConnections();
            
            foreach (var con in connections)
            {
                this.DropConnection(con);
            }
        }

        public void DropConnection(Connection con)
        {
            var client = this.CreateClient();
            var dropRequest = CreateRequest($"/api/connections/{con.name}", Method.DELETE);
            var response = client.Execute(dropRequest);

            if (response.ErrorException != null)
            {
                throw new Exception("Failed to drop a connection", response.ErrorException);
            }
        }

        public IList<VirtualHost> GetHosts()
        {
            var client = this.CreateClient();
            var request = CreateRequest("/api/vhosts", Method.GET);
            
            var response = client.Execute<List<VirtualHost>>(request);
            return response.Data;
        }
    }
}
