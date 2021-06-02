using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HMACClient
{
    class Program
    {
        static void Main(string[] args)
        {
            RunAsync().Wait();
            Console.ReadLine();
        }
        static async Task RunAsync()
        {
            Console.WriteLine("Calling the back-end API");

            // need to change the port number
            // provide the port number where your api is running
            var apiBaseAddress = "http://localhost:49626/";

            HMACDelegatingHandler hMACDelegatingHandler = new()
            {
                MyProperty = 56
            };
            HttpClient httpClient = HttpClientFactory.Create(hMACDelegatingHandler);

            var order = new Order
            {
                OrderId = 10320,
                CustomerName = "Sohrab",
                CustomerAddress = "Kabul | Afghanistan | AF",
                ContactNumber = "0780052052",
                IsShipped = true
            };

            // hash the body with secret key.
            var md5 = MD5.Create();
            var bodyBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(order));
            var bodyHash = md5.ComputeHash(bodyBytes);

            // get the byte array from the sharedkey 
            var secretBytes = Convert.FromBase64String("kwAg1/mZSjz34iC0nR9Luy4yP6Fhxqr0udk1kTwUSjM=");

            var hashedBody = string.Empty;

            using(var hmac = new HMACSHA256(secretBytes))
            {
                var signatureBytes = hmac.ComputeHash(bodyHash);
                hashedBody = Convert.ToBase64String(signatureBytes);
            }

            var encryptedBody = new
            {
                Content = hashedBody
            };

            // call to a POST request
            HttpResponseMessage response = await httpClient.PostAsJsonAsync(apiBaseAddress + "api/order", encryptedBody);

            if (response.IsSuccessStatusCode)
            {
                string responseString = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseString);
                Console.WriteLine("HTTP Status: {0}, Reason {1}. Press Enter to exit", response.StatusCode, response.ReasonPhrase);
            }
            else
            {
                Console.WriteLine("Failed to call the API. HTTP Status: {0}, Reason {1}", response.StatusCode, response.ReasonPhrase);
            }

            // call to a GET request
            var getResponse = await httpClient.GetAsync(apiBaseAddress + "api/order");
            if (getResponse.IsSuccessStatusCode)
            {
                string responseString = await getResponse.Content.ReadAsStringAsync();
                Console.WriteLine(responseString);
                Console.WriteLine("HTTP Status: {0}, Reason {1}. Press Enter to exit", getResponse.StatusCode, getResponse.ReasonPhrase);
            }
            else
            {
                Console.WriteLine("Failed to call the API. HTTP Status: {0}, Reason {1}", getResponse.StatusCode, getResponse.ReasonPhrase);
            }
        }
    }
}
