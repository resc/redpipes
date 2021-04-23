using System;
using RabbitMQ.Client;

namespace RedPipes.RabbitMQ
{
    /// <summary> Represents a RabbitMQ message receive event </summary>
    public class Receive
    {
        /// <summary> </summary>
        public string ConsumerTag { get; }
        /// <summary> </summary>
        public ulong DeliveryTag { get; }
        /// <summary> </summary>
        public bool Redelivered { get; }
        /// <summary> </summary>
        public string Exchange { get; }
        /// <summary> </summary>
        public string RoutingKey { get; }
        /// <summary> </summary>
        public IBasicProperties Properties { get; }
        /// <summary> </summary>
        public ReadOnlyMemory<byte> Body { get; }

        /// <summary> </summary>
        public Receive(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, in ReadOnlyMemory<byte> body)
        {
            ConsumerTag = consumerTag;
            DeliveryTag = deliveryTag;
            Redelivered = redelivered;
            Exchange = exchange;
            RoutingKey = routingKey;
            Properties = properties;
            Body = body;
        }
    }
}
