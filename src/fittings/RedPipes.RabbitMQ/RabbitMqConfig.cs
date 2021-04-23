namespace RedPipes.RabbitMQ
{
    /// <summary> configuration for RabbitMQ connection and model </summary>
    public class RabbitMqConfig
    {
        private ConnectionConfig? _connection;
        private ModelConfig? _model;

        /// <summary> the connection configuration </summary>
        public ConnectionConfig Connection
        {
            get { return _connection ??= new ConnectionConfig(); }
            set { _connection = value; }
        }

        /// <summary> the model configuration </summary>
        public ModelConfig Model
        {
            get { return _model ??= new ModelConfig(); }
            set { _model = value; }
        }
    }
}
