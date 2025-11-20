using ModularERP.Common.Enum.Finance_Enum;

namespace ModularERP.Common.Exceptions
{
    public abstract class BaseApplicationException : Exception
    {
        public string Module { get; protected set; }
        public FinanceErrorCode FinanceErrorCode { get; protected set; }
        public int HttpStatusCode { get; protected set; }

        protected BaseApplicationException(string message, string module, FinanceErrorCode financeErrorCode, int httpStatusCode)
            : base(message)
        {
            Module = module;
            FinanceErrorCode = financeErrorCode;
            HttpStatusCode = httpStatusCode;
        }

        protected BaseApplicationException(string message, Exception innerException, string module, FinanceErrorCode financeErrorCode, int httpStatusCode)
            : base(message, innerException)
        {
            Module = module;
            FinanceErrorCode = financeErrorCode;
            HttpStatusCode = httpStatusCode;
        }
    }
}
