using System;
using System.Collections.Generic;

namespace RedPipes.Patterns.Rpc
{
    /// <summary>  </summary>
    public class RpcOptions
    {
        /// <summary>  </summary>
        public static RpcOptions Default
        {
            get
            {
                return new RpcOptions
                {
                    EndPoint = "",
                    Timeout = TimeSpan.FromSeconds(60), 
                };
            }
        }

        /// <summary>  </summary>
        public string EndPoint { get; set; } = "";
        
        /// <summary>  </summary>
        public TimeSpan Timeout { get; set; }

        /// <summary>  </summary>
        public IDictionary<string, object> Config { get; set; } = new Dictionary<string, object>();

        /// <summary>  </summary>
        public RpcOptions Clone()
        {
            return new RpcOptions
            {
                Timeout = Timeout,
                EndPoint = EndPoint,
                Config = new Dictionary<string, object>(Config),
            };
        }
    }
}
