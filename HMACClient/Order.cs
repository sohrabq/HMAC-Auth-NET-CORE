namespace HMACClient
{
    public class Order
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        public string ContactNumber { get; set; }
        public bool IsShipped { get; set; }
    }
}
