namespace RedPipes.RabbitMQ
{
    public class ExchangeDeclaration : Declaration
    {
        public string Type { get; set; }
        public bool Durable { get; set; }
        public bool AutoDelete { get; set; }
    }
}