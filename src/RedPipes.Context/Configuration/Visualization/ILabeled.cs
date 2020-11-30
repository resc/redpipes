using System.Collections.Generic;

namespace RedPipes.Configuration.Visualization
{
    public interface ILabeled
    {
        int Id { get; }
        IDictionary<string, object> Labels { get; }
    }
}