namespace RedPipes.RabbitMQ
{
    public class ExchangeConfig
    {
        private ExchangeDeclaration _declaration;
        public string Name { get; set; }

        public ExchangeDeclaration Declaration
        {
            get { return _declaration ?? (_declaration = new ExchangeDeclaration()); }
            set { _declaration = value; }
        }
    }
}