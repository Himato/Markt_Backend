
namespace Markt.Models
{
    public class BillingAddress
    {
        public int Id { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string PostCode { get; set; }

        public string Country { get; set; }

        public string UserId { get; set; }

        public ApplicationUser ApplicationUser { get; set; }
    }
}
