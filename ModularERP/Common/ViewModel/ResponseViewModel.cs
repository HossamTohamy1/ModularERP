﻿using ModularERP.Common.Enum.Finance_Enum;

namespace ModularERP.Common.ViewModel
{
    public class ResponseViewModel<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public FinanceErrorCode? FinanceErrorCode { get; set; }
        public string TraceId { get; set; }
        public Dictionary<string, string[]> ValidationErrors { get; set; }
        public DateTime Timestamp { get; set; }

        public ResponseViewModel()
        {
            Timestamp = DateTime.UtcNow;
        }

        public static ResponseViewModel<T> Success(T data, string message = "Operation completed successfully")
        {
            return new ResponseViewModel<T>
            {
                IsSuccess = true,
                Message = message,
                Data = data
            };
        }

        public static ResponseViewModel<T> Error(string message, Enum.Finance_Enum.FinanceErrorCode errorCode, string traceId = null)
        {
            return new ResponseViewModel<T>
            {
                IsSuccess = false,
                Message = message,
                FinanceErrorCode = errorCode,
                TraceId = traceId
            };
        }

        public static ResponseViewModel<T> ValidationError(string message, Dictionary<string, string[]> validationErrors, string traceId = null)
        {
            return new ResponseViewModel<T>
            {
                IsSuccess = false,
                Message = message,
                FinanceErrorCode = Enum.Finance_Enum.FinanceErrorCode.ValidationError,
                ValidationErrors = validationErrors,
                TraceId = traceId
            };
        }
    }
}