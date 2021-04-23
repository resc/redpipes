namespace RedPipes.RabbitMQ
{
    /// <summary>  </summary>
    public class QueueDeclaration : Declaration
    {
        /// <summary>  </summary>
        public bool Durable { get; set; }
        /// <summary>  </summary>
        public bool Exclusive { get; set; }
        /// <summary>  </summary>
        public bool AutoDelete { get; set; } = true;
    }
}
