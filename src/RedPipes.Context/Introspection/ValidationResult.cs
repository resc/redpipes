namespace RedPipes.Introspection
{
    public class ValidationResult
    {
        public ValidationResult(bool isSuccess, string message)
        {
            IsSuccess = isSuccess;
            Message = message;
        }

        public bool IsSuccess { get; }

        public string Message { get; }
    }
}