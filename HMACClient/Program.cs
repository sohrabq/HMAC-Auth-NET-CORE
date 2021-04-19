using System;
using System.Net.Http;
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

            HMACDelegatingHandler hMACDelegatingHandler = new();
            HttpClient httpClient = HttpClientFactory.Create(hMACDelegatingHandler);

            var order = new Order
            {
                OrderId = 10320,
                CustomerName = "Sohrab",
                CustomerAddress = "Kabul | Afghanistan | AF",
                ContactNumber = "0780052052",
                IsShipped = true
            };

            // call to a POST request
            HttpResponseMessage response = await httpClient.PostAsJsonAsync(apiBaseAddress + "api/order", order);

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
