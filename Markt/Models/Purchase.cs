
namespace Markt.Models
{
    public class Purchase
    {
        public int Id { get; set; }

        public int Quantity { get; set; }

        public double TotalPrice { get; set; }

        public bool IsFinished { get; set; }

        public int ProductId { get; set; }

        public int? OrderId { get; set; }

        public string UserId { get; set; }

        public Product Product { get; set; }

        public Order Order { get; set; }

        public ApplicationUser User { get; set; }
    }
}
