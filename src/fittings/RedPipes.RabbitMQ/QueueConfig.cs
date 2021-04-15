namespace RedPipes.RabbitMQ
{
    /// <summary> queue configuration </summary>
    public class QueueConfig
    {
        private QueueDeclaration? _declaration;

        private ConsumerDeclaration? _consume;

        /// <summary> queue name </summary>
        public string Name { get; set; } = "";

        /// <summary> queue declaration  </summary>
        public QueueDeclaration Declaration
        {
            get { return _declaration ??= new QueueDeclaration(); }
            set { _declaration = value; }
        }

        /// <summary> queue consumer configuration </summary>
        public ConsumerDeclaration Consumer
        {
            get { return _consume ??= new ConsumerDeclaration(); }
            set { _consume = value; }
        }
    }
}
