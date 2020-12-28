using System;
using System.Collections.Generic;

namespace RedPipes.Patterns.Rpc
{
    public class RpcOptions
    {
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

        public string EndPoint { get; set; } = "";
        
        public TimeSpan Timeout { get; set; }

        public IDictionary<string, object> Config { get; set; } = new Dictionary<string, object>();

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
