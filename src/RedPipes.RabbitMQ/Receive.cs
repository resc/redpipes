using System;
using RabbitMQ.Client;

namespace RedPipes.RabbitMQ
{
    public class Receive
    {
        public string ConsumerTag { get; }
        public ulong DeliveryTag { get; }
        public bool Redelivered { get; }
        public string Exchange { get; }
        public string RoutingKey { get; }
        public IBasicProperties Properties { get; }
        public ReadOnlyMemory<byte> Body { get; }

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