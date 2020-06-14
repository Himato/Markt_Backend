using System;
using System.Collections.Generic;

namespace Markt.Models
{
    public class Product
    {
        public int Id { get; set; }

        public string Uri { get; set; }
        
        public string Name { get; set; }

        public string Description { get; set; }

        public string Specification { get; set; }

        public string ReturnInfo { get; set; }

        public DateTime DateTime { get; set; }

        public float Price { get; set; }

        public bool IsInStock { get; set; }

        public int SubcategoryId { get; set; }

        public string SellerId { get; set; }

        public int BrandId { get; set; }

        public ICollection<Image> Images { get; set; }

        public ICollection<Review> Reviews { get; set; }

        public Subcategory Subcategory { get; set; }

        public ApplicationUser Seller { get; set; }

        public Brand Brand { get; set; }
    }
}
