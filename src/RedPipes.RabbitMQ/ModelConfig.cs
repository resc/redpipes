using System;
using System.Collections.Generic;
using System.Linq;
using RabbitMQ.Client;

namespace RedPipes.RabbitMQ
{
    public class ModelConfig
    {
        public Action<IModel> Configure { get; set; }

        public IEnumerable<ExchangeConfig> Exchanges { get; set; } = Enumerable.Empty<ExchangeConfig>();
        public IEnumerable<QueueConfig> Queues { get; set; } = Enumerable.Empty<QueueConfig>();
    }
}