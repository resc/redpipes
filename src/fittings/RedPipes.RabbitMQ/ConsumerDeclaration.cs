namespace RedPipes.RabbitMQ
{
    /// <summary> Declaration information for a RabbitMQ consumer </summary>
    public class ConsumerDeclaration : Declaration
    {
        /// <summary> autoack for the consumer </summary>
        public bool AutoAck { get; set; }

        /// <summary> the tag for the consumer </summary>
        public string ConsumerTag { get; set; } = "";

        /// <summary> the nolocal setting for the consumer </summary>
        public bool NoLocal { get; set; }

        /// <summary> the exclusive setting for the consumer </summary>
        public bool Exclusive { get; set; }
    }
}
