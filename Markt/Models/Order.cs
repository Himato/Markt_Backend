using System;

namespace Markt.Models
{
    public class Order
    {
        public int Id { get; set; }

        public DateTime DateTime { get; set; }

        public double TotalPrice { get; set; }

        public int BillingAddressId { get; set; }

        public string UserId { get; set; }

        public BillingAddress BillingAddress { get; set; }

        public ApplicationUser User { get; set; }
    }
}
