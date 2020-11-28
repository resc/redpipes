namespace RedPipes.RabbitMQ
{
    public class ConsumerDeclaration : Declaration
    {
        public bool AutoAck { get; set; }
        public string ConsumerTag { get; set; }
        public bool NoLocal { get; set; }
        public bool Exclusive { get; set; }
    }
}