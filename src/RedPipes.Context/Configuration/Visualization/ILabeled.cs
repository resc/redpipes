using System.Collections.Generic;

namespace RedPipes.Configuration.Visualization
{
    public interface ILabeled
    {
        int Id { get; }
        string Name { get; }
        IDictionary<string, object> Labels { get; }
    }
}
