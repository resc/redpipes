using System.Collections.Generic;

namespace RedPipes.RabbitMQ
{
    /// <summary> Base class for the RabbitMQ model declarations </summary>
    public abstract class Declaration
    {
        private Dictionary<string, object>? _arguments;
        
        /// <summary> Declaration arguments </summary>
        public Dictionary<string, object> Arguments
        {
            get { return _arguments ??= new Dictionary<string, object>(); }
            set { _arguments = value; }
        }
    }
}
