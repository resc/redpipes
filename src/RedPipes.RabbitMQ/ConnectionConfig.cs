using System;
using System.Collections.Generic;
using RabbitMQ.Client;

namespace RedPipes.RabbitMQ
{
    public class ConnectionConfig
    {
        public Uri Uri { get; set; }

        public string ClientProvidedName { get; set; }

        private IDictionary<string, object> _clientProperties;
        public IDictionary<string, object> ClientProperties
        {
            get
            {
                return _clientProperties ?? (_clientProperties = new Dictionary<string, object>());
            }
            set
            {
                _clientProperties = value;
            }
        }

        public Action<IConnectionFactory> ConfigureFactory { get; set; }

        public Action<IConnection> Configure { get; set; }
    }
}