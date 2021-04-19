using HMACAuthenticationWebAPI.Filters;
using Microsoft.AspNetCore.Mvc;

namespace HMACAuthenticationWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        [HttpGet()]
        [TypeFilter(typeof(HMACAuthAttribute))]
        public IActionResult Get()
        {
            return Ok(Order.GetOrders());
        }
        [HttpPost()]
        [TypeFilter(typeof(HMACAuthAttribute))]
        public IActionResult Post(Order order)
        {
            return Ok(order);
        }
    }
}
