using System.Collections.Generic;

namespace RedPipes.Introspection
{
    /// <summary> this object can validate </summary>
    public interface IValidator
    {
        /// <summary> Validates, and returns the results.
        /// If no results are returned successful validation is implied.  </summary>
        IEnumerable<ValidationResult> Validate();
    }
}