namespace RedPipes.RabbitMQ
{
    /// <summary>  </summary>
    public class ExchangeConfig
    {
        private ExchangeDeclaration? _declaration;

        /// <summary> this exchange name </summary>
        public string Name { get; set; } = "";

        /// <summary> the exchange declaration </summary>
        public ExchangeDeclaration Declaration
        {
            get { return _declaration ??= new ExchangeDeclaration(); }
            set { _declaration = value; }
        }
    }
}
