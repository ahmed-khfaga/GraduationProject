namespace FitZone.APIs.Errors
{
    public class ApiExceptionServer : ApiException
    {
        public string? Details { set; get; }

        public ApiExceptionServer(int statuscode, string? message=null,string? details=null):base(statuscode,message)
        {
            Details = details;
        }
    }
}
