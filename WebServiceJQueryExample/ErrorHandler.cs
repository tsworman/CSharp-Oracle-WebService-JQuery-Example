using System;

namespace WebServiceJQueryExample
{
    public class ErrorHandler
    {
        public static void CreateError(Response NewResponse, Exception ex, String ErrorType, String Method)
        {
            NewResponse.Success = false;
            NewResponse.Caller = Method;
            NewResponse.ErrorType = ErrorType;
            NewResponse.Message = "Error: " + ex.Message;
        }
    }
}