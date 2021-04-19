using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace HMACClient
{
    public class HMACDelegatingHandler : DelegatingHandler
    {
        // first obtained the API ID and API key from the server
        // the APIKEY must be stored securely in db or in the App.Config or AppSetting.JSON.
        private readonly string APPId = "ad644212-adc0-44c7-a4b1-9f2ff06d550a";
        private readonly string APIKey = "kwAg1/mZSjz34iC0nR9Luy4yP6Fhxqr0udk1kTwUSjM=";
        private readonly string AuthScheme = "hmacauth";

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, 
            CancellationToken cancellationToken)
        {
            string requestContentBase64String = string.Empty;

            // get the request URI
            string requestUri = HttpUtility.UrlEncode(request.RequestUri.AbsoluteUri.ToLower());

            // get the request HTTP method type
            string requestHttpMethod = request.Method.Method;

            // calculate UNIX time
            DateTime epochStart = new DateTime(1970, 01, 01, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan timeSpan = DateTime.UtcNow - epochStart;
            string requestTimestamp = Convert.ToUInt64(timeSpan.TotalSeconds).ToString();

            // create the random nonce for each request
            string nonce = Guid.NewGuid().ToString("N");
            var _content = string.Empty;
            if (requestHttpMethod != "GET")
                 _content = await request?.Content?.ReadAsStringAsync(cancellationToken);

            // checking if the request contains body, usually will be null with HTTP GET and DELETE
            if(request.Content != null)
            {
                // Hashing the request body, so any change in request body will result a different hash
                // we will achieve message itegrity
                byte[] content = Encoding.UTF8.GetBytes(_content);
                MD5 mD5 = MD5.Create();
                byte[] requestContentHash = mD5.ComputeHash(content);
                requestContentBase64String = Convert.ToBase64String(requestContentHash);
            }

            // creating the raw signature string by combining APPId, request Http Method, request Uri, requst TimeStamp, nonce,
            // request Content Base64 String.
            string signatureRawData = string.Format("{0}{1}{2}{3}{4}{5}", APPId,
                requestHttpMethod, requestUri, requestTimestamp, nonce, requestContentBase64String);

            // converting the APIKey into byte array 
            byte[] secretKeyByteArray = Convert.FromBase64String(APIKey);
            
            // converting the signatureRawData into byte array
            byte[] signature = Encoding.UTF8.GetBytes(signatureRawData);

            // generate the hmac signature and set it in the authorization header
            using (var hmac = new HMACSHA256(secretKeyByteArray))
            {
                byte[] signatureBytes = hmac.ComputeHash(signature);
                string requestSignatureBase64String = Convert.ToBase64String(signatureBytes);

                // Setting the values in the authorization header using custom scheme (hmacauth)
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(AuthScheme,
                    string.Format("{0}:{1}:{2}:{3}", APPId, requestSignatureBase64String, nonce, requestTimestamp));
            }

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
            return response;
        }
    }
}
