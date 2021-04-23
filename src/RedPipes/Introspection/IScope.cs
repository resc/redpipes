using System.ComponentModel;

namespace RedPipes.Introspection
{
    /// <summary>
    /// An inspection scope is used to collect the object attributes
    /// </summary>
    public interface IScope
    {
        /// <summary> Adds a child scope with the given name </summary>
        /// <param name="name"></param>
        IScope Scope(string name);

        /// <summary> Sets a scope attribute, any non-primitive values will be serialized
        /// using the <see cref="TypeConverter.ConvertToInvariantString(object)"/>,
        /// using the converter retrieved from <see cref="TypeDescriptor.GetConverter(object)"/> </summary>
        IScope Attr(string name, object value);
    }
}