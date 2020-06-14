
namespace Markt.Models
{
    public class Review
    {
        public int Id { get; set; }

        public int Rate { get; set; }

        public int ProductId { get; set; }

        public string UserId { get; set; }

        public Product Product { get; set; }

        public ApplicationUser User { get; set; }
    }
}
