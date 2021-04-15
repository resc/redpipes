using System;
using System.Collections.Generic;
using RabbitMQ.Client;

namespace RedPipes.RabbitMQ
{
    /// <summary> RabbitMQ connection configuration </summary>
    public class ConnectionConfig
    {
        /// <summary> the connection uri </summary>
        public Uri Uri { get; set; } = new Uri("amqp://guest:guest@localhost:5672");

        /// <summary> the name provided to RabbitMQ for this client </summary>
        public string ClientProvidedName { get; set; } = "";

        private IDictionary<string, object>? _clientProperties;

        public IDictionary<string, object> ClientProperties
        {
            get { return _clientProperties ??= new Dictionary<string, object>(); }
            set { _clientProperties = value; }
        }

        /// <summary> Configure the RabbitMQ before creating a connection </summary>
        public Action<IConnectionFactory> ConfigureFactory { get; set; } = f => { };

        /// <summary> Configures the connection before it is opened </summary>
        public Action<IConnection> Configure { get; set; } = c => { };
    }
}
