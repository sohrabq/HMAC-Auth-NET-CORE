using System.Collections.Generic;

namespace HMACAuthenticationWebAPI
{
    public class Order
    {
        public int OrderID { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        public string ContactNumber { get; set; }
        public bool IsShipped { get; set; }
        public static List<Order> GetOrders()
        {
            List<Order> OrderList = new ()
            {
                new Order {OrderID = 101, CustomerName = "Pranaya", CustomerAddress = "Amman", ContactNumber = "9876543210", IsShipped = true },
                new Order {OrderID = 102, CustomerName = "Anurag", CustomerAddress = "Dubai",ContactNumber = "9876543210", IsShipped = false},
                new Order {OrderID = 103, CustomerName = "Priyanka", CustomerAddress = "Jeddah", ContactNumber = "9876543210", IsShipped = false },
                new Order {OrderID = 104, CustomerName = "Hina", CustomerAddress = "Abu Dhabi",ContactNumber = "9876543210", IsShipped = false},
                new Order {OrderID = 104, CustomerName = "Sambit", CustomerAddress = "Kuwait", ContactNumber = "9876543210",IsShipped = true}
            };
            return OrderList;
        }
    }
}
