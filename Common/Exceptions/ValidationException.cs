namespace Common.Exceptions
{
    public class ValidationException : Exception
    {
        public IEnumerable<string> ValidationErrors { get; }

        public ValidationException(IEnumerable<string> validationErrors)
            : base("One or more validation errors occurred.")
        {
            ValidationErrors = validationErrors ?? Enumerable.Empty<string>();
        }
    }
}
