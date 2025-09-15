using ModularERP.Common.Enum.Finance_Enum;
using Microsoft.AspNetCore.Http;

namespace ModularERP.Common.Exceptions
{
    public class NotFoundException : BaseApplicationException
    {
        private const string DefaultModule = "Finance";

        public NotFoundException(string message, FinanceErrorCode errorCode)
            : base(message, DefaultModule, errorCode, StatusCodes.Status404NotFound)
        {
        }

        public NotFoundException(string message, FinanceErrorCode errorCode, Exception innerException)
            : base(message, innerException, DefaultModule, errorCode, StatusCodes.Status404NotFound)
        {
        }
    }
}
