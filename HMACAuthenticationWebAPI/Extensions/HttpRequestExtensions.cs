using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace HMACAuthenticationWebAPI.Extensions
{
    public static class HttpRequestExtensions
    {
        public static async Task<string> BodyToString(this HttpRequest request)
        {
            var body = string.Empty;
            request.EnableBuffering();
            request.Body.Position = 0;
            using (var stream = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true))
            {
                body = await stream.ReadToEndAsync();
            }
            request.Body.Position = 0;
            return body;
        }
    } 
}
