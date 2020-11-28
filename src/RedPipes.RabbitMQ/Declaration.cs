using System.Collections.Generic;

namespace RedPipes.RabbitMQ
{
    public abstract class Declaration
    {
        private Dictionary<string, object> _arguments;
        public Dictionary<string, object> Arguments
        {
            get { return _arguments ?? (_arguments = new Dictionary<string, object>()); }
            set { _arguments = value; }
        }
    }
}