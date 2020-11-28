namespace RedPipes.RabbitMQ
{
    public class RabbitMqConfig
    {
        private ConnectionConfig _connection;
        private ModelConfig _model;

        public ConnectionConfig Connection
        {
            get { return _connection ?? (_connection = new ConnectionConfig()); }
            set { _connection = value; }
        }

        public ModelConfig Model
        {
            get { return _model ?? (_model = new ModelConfig()); }
            set { _model = value; }
        }
    }
}