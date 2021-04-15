namespace RedPipes.Introspection
{
    /// <summary> result for a validation of the pipe structure </summary>
    public class ValidationResult
    {
        /// <summary> creates a new validation result </summary>
        public ValidationResult(bool isSuccess, string message)
        {
            IsSuccess = isSuccess;
            Message = message;
        }

        /// <summary> true if the validation succeeded </summary>
        public bool IsSuccess { get; }

        /// <summary> the validation message, can contain some praise when <see cref="IsSuccess"/> is true </summary>
        public string Message { get; }
    }
}
