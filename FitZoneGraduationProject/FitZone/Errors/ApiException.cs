using Microsoft.EntityFrameworkCore.Query.Internal;

namespace FitZone.APIs.Errors
{
    public class ApiException
    {
        public int StatusCode { get; }

        public string? Message { get; }


        public ApiException(int statusCode , string? message = null)
        {
            StatusCode = statusCode ;

            Message = message ?? GetValueMessageForStatusCode(statusCode) ;
        }

        private string ? GetValueMessageForStatusCode(int statusCode)
        {
            return statusCode switch
            {
                400 => "Bad Request",
                401 => "Unauthroized , you are not",
                404 => "Resource was not found",
                500 => "Errors are the path to the dark side. Errors leads to anger. Anger leads to hate . Hate leads to career change",
                _ => null
            };
        }
    }
}
