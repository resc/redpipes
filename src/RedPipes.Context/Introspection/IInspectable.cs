namespace RedPipes.Introspection
{
    /// <summary> this object is inspectable </summary>
    public interface IInspectable
    {
        /// <summary> Adds  attributes to the scope for this object </summary>
        void Inspect(IScope scope);
    }
}
