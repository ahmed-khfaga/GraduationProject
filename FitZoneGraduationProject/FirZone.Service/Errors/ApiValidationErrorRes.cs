namespace FitZone.Service.Errors
{
    public class ApiValidationErrorRes : ApiException
    {
        public IEnumerable<string> Errors { get; set; }

        public ApiValidationErrorRes():base(400)
        {
            Errors = new List<string>();
        }
    }
}
