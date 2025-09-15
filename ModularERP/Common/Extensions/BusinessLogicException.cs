using ModularERP.Common.Enum.Finance_Enum;

namespace ModularERP.Common.Exceptions
{
    public class BusinessLogicException : BaseApplicationException
    {
        public BusinessLogicException(string message, string module, FinanceErrorCode financeErrorCode = FinanceErrorCode.BusinessLogicError)
            : base(message, module, financeErrorCode, StatusCodes.Status400BadRequest)
        {
        }

        public BusinessLogicException(string message, Exception innerException, string module, FinanceErrorCode financeErrorCode = FinanceErrorCode.BusinessLogicError)
            : base(message, innerException, module, financeErrorCode, StatusCodes.Status400BadRequest)
        {
        }
    }
}