using System;
using System.Collections.Generic;
using System.Linq;
using RabbitMQ.Client;

namespace RedPipes.RabbitMQ
{
    /// <summary> Configuration for the <see cref="IModel"/> </summary>
    public class ModelConfig
    {
        /// <summary> Advanced configuration function for the model </summary>
        public Action<IModel> Configure { get; set; } = m => { };

        /// <summary> all configured exchanges </summary>
        public IEnumerable<ExchangeConfig> Exchanges { get; set; } = Array.Empty<ExchangeConfig>();

        /// <summary> all configured queues </summary>
        public IEnumerable<QueueConfig> Queues { get; set; } = Array.Empty<QueueConfig>();
    }
}
