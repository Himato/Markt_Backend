using System.ComponentModel.DataAnnotations;
using Markt.Models;

namespace Markt.Dtos
{
    public class BrandDto
    {
        [Required]
        [MinLength(2)]
        public string Name { get; set; }
    }

    public class AdminBrandDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int NumberOfProducts { get; set; }

        public static AdminBrandDto Create(Brand brand, int numberOfProducts)
        {
            return new AdminBrandDto
            {
                Id = brand.Id,
                Name = brand.Name,
                NumberOfProducts = numberOfProducts
            };
        }
    }
}
