namespace RedPipes.RabbitMQ
{
    public class QueueConfig
    {
        private QueueDeclaration _declaration;
        private ConsumerDeclaration _consume;

        public string Name { get; set; }

        public QueueDeclaration Declaration
        {
            get { return _declaration ?? (_declaration = new QueueDeclaration()); }
            set { _declaration = value; }
        }

        public ConsumerDeclaration Consumer
        {
            get { return _consume ?? (_consume = new ConsumerDeclaration()); }
            set { _consume = value; }
        }
    }
}