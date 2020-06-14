using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Markt.Models;

namespace Markt.Dtos
{
    public class CategoryDto
    {
        [Required]
        [MinLength(4, ErrorMessage = "The name of the category can't be less than 4 characters")]
        public string Name { get; set; }
    }

    public class AdminCategoryDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int NumberOfSubcategories { get; set; }

        public int NumberOfProducts { get; set; }

        public static AdminCategoryDto Create(Category category, int numberOfSubcategories, int numberOfProducts)
        {
            return new AdminCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                NumberOfSubcategories = numberOfSubcategories,
                NumberOfProducts = numberOfProducts
            };
        }
    }

    public class SubcategoryDto
    {
        [Required]
        [MinLength(4, ErrorMessage = "The name of the subcategory can't be less than 4 characters")]
        public string Name { get; set; }
        
        public int CategoryId { get; set; }
    }

    public class AdminSubcategoryDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int NumberOfProducts { get; set; }

        public static AdminSubcategoryDto Create(Subcategory subcategory, int numberOfProducts)
        {
            return new AdminSubcategoryDto
            {
                Id = subcategory.Id,
                Name = subcategory.Name,
                NumberOfProducts = numberOfProducts
            };
        }
    }

    public class AdminSubcategoriesDto
    {
        public IEnumerable<AdminSubcategoryDto> Subcategories { get; set; }

        public Category Category { get; set; }

        public static AdminSubcategoriesDto Create(Category category, List<AdminSubcategoryDto> subcategories)
        {
            return new AdminSubcategoriesDto
            {
                Subcategories = subcategories,
                Category = category
            };
        }
    }
}
