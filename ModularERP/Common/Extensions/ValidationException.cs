using ModularERP.Common.Enum.Finance_Enum;

namespace ModularERP.Common.Exceptions
{
    public class ValidationException : BaseApplicationException
    {
        public Dictionary<string, string[]> ValidationErrors { get; }

        public ValidationException(string message, Dictionary<string, string[]> validationErrors, string module = "Common")
            : base(message, module, FinanceErrorCode.ValidationError, StatusCodes.Status400BadRequest)
        {
            ValidationErrors = validationErrors ?? new Dictionary<string, string[]>();
        }

        public ValidationException(string message, Dictionary<string, string[]> validationErrors, Exception innerException, string module = "Common")
            : base(message, innerException, module, FinanceErrorCode.ValidationError, StatusCodes.Status400BadRequest)
        {
            ValidationErrors = validationErrors ?? new Dictionary<string, string[]>();
        }
    }
}