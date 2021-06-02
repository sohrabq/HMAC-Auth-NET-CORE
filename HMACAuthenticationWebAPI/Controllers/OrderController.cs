using HMACAuthenticationWebAPI.Encryption;
using HMACAuthenticationWebAPI.Filters;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace HMACAuthenticationWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly AesHelper aesHelper;

        public OrderController(AesHelper aesHelper)
        {
            this.aesHelper = aesHelper;
        }
        [HttpGet()]
        [TypeFilter(typeof(HMACAuthAttribute))]
        public IActionResult Get()
        {
            return Ok(Order.GetOrders());
        }
        [HttpPost()]
        [TypeFilter(typeof(HMACAuthAttribute))]
        public IActionResult Post([FromBody] Body order)
        {
            var request = HttpContext;
            var result = aesHelper.Decrypt(order.Content);
            Console.WriteLine(result);
            return Ok(order);
        }

    }
    public class Body
    {
        public string Content { get; set; }
    }
}
