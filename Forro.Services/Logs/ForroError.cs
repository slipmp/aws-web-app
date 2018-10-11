using Newtonsoft.Json;

namespace Forro.Services.Logs
{
    public class ForroErrorDetails
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }


        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
