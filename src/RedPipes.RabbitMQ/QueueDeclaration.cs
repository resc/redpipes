namespace RedPipes.RabbitMQ
{
    public class QueueDeclaration : Declaration
    {
        public bool Durable { get; set; }
        public bool Exclusive { get; set; }
        public bool AutoDelete { get; set; } = true;
    }
}