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

        public string EndPoint { get; set; }

        public TimeSpan Timeout { get; set; }

        public IDictionary<string, object> Config { get; set; }

        public RpcOptions Clone()
        {
            var config = Config;
            if (config != null)
                config = new Dictionary<string, object>(config);

            return new RpcOptions
            {
                Timeout = Timeout,
                EndPoint = EndPoint,
                Config = config,
            };
        }
    }
}
