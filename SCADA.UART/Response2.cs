using System;

namespace SCADA.UART
{
    public class Response2
    {
        public Response2()
        {
            ErrorMsg = string.Empty;
            Exception = null;
        }

        public Response2(string errorMsg)
        {
            ErrorMsg = errorMsg;
            Exception = null;
        }

        public Response2(Exception ex)
        {
            Exception = ex;
            ErrorMsg = string.Empty;
        }

        public Response2(Exception ex, string errorMsg)
        {
            ErrorMsg = errorMsg;
            Exception = ex;
        }

        public string ErrorMsg { get; }
        public Exception Exception { get; }
        public bool IsSuccess => Exception == null && string.IsNullOrWhiteSpace(ErrorMsg);
    }

    public class Response<T> : Response2
    {
        public Response(T data)
        {
            Data = data;
        }

        public Response(T data, string errorMsg) : base(errorMsg)
        {
            Data = data;
        }

        public Response(T data, Exception ex) : base(ex)
        {
            Data = data;
        }

        public Response(T data, Exception ex, string errorMsg) : base(ex, errorMsg)
        {
            Data = data;
        }

        public T Data { get; }
    }
}