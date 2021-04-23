using System.Collections.Generic;

namespace RedPipes.Configuration.Visualization
{
    /// <summary> the interface for items with labels </summary>
    public interface ILabeled
    {
        /// <summary> item id </summary>
        int Id { get; }

        /// <summary> item name </summary>
        string Name { get; }

        /// <summary> the items labels </summary>
        IDictionary<string, object> Labels { get; }
    }
}
