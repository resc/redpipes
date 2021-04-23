namespace RedPipes.RabbitMQ
{
    /// <summary> the RabbitMQ exchange declaration information </summary>
    public class ExchangeDeclaration : Declaration
    {
        /// <summary> the exchange type </summary>
        public string Type { get; set; } = global::RabbitMQ.Client.ExchangeType.Direct;
        /// <summary> durability of the exchange </summary>
        public bool Durable { get; set; } = false;
        /// <summary> auto delete the exchange when it's not used anymore. </summary>
        public bool AutoDelete { get; set; } = true;
    }
}
