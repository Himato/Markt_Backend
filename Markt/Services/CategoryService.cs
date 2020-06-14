using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Markt.Dtos;
using Markt.Helpers;
using Markt.Models;
using Microsoft.EntityFrameworkCore;

namespace Markt.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAll();

        Task<IEnumerable<AdminCategoryDto>> GetCategoriesForAdmin();

        Task<Category> GetCategory(int id);

        /// <returns>Category Id to add subcategories</returns>
        Task<int> AddCategory(string name);

        Task UpdateCategory(int id, string name);

        Task DeleteCategory(int id);

        Task<Subcategory> GetSubcategoryByUri(string subcategoryUri);

        Task<IEnumerable<AdminSubcategoriesDto>> GetSubcategoriesForAdmin();

        Task<IEnumerable<Subcategory>> GetSubcategories();

        Task<Subcategory> GetSubcategory(int id);

        /// <returns>Uri</returns>
        Task<int> AddSubcategory(int categoryId, string name);

        /// <returns>New Uri</returns>
        Task<int> UpdateSubcategory(int id, int categoryId, string name);

        Task DeleteSubcategory(int id);
    }

    public class CategoryService : ServiceHelper, ICategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async  Task<IEnumerable<Category>> GetAll()
        {
            var categories = await GetOrderedCategories();

            foreach (var category in categories)
            {
                category.Subcategories = category.Subcategories.OrderBy(c => c.Name).ToList();
            }

            return categories;
        }

        public async Task<IEnumerable<AdminCategoryDto>> GetCategoriesForAdmin()
        {
            var categories = await GetOrderedCategories();

            var output = new List<AdminCategoryDto>();

            foreach (var category in categories)
            {
                var subcategories = category.Subcategories.Select(s => s.Id).ToList();

                var numberOfProducts = await _context.Products
                    .CountAsync(p => subcategories.Contains(p.SubcategoryId));

                output.Add(AdminCategoryDto.Create(category, subcategories.Count, numberOfProducts));
            }

            return output;
        }

        public async Task<Category> GetCategory(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public async Task<int> AddCategory(string name)
        {
            if (await DoesCategoryExist(name))
            {
                throw new ArgumentException("This name already exists");
            }

            var category = new Category
            {
                Name = name
            };

            await Do(async () => await _context.Categories.AddAsync(category));

            return category.Id;
        }

        public async Task UpdateCategory(int id, string name)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                throw new KeyNotFoundException("Category not found");
            }

            if (await DoesCategoryExist(name))
            {
                throw new ArgumentException("This name already exists");
            }

            await Do(() =>
            {
                category.Name = name;
                _context.Entry(category).State = EntityState.Modified;
            });
        }

        public async Task DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                throw new KeyNotFoundException("Category not found");
            }

            await Do(() => _context.Categories.Remove(category));
        }

        public async Task<Subcategory> GetSubcategoryByUri(string subcategoryUri)
        {
            return await _context.Subcategories.FirstOrDefaultAsync(c => c.Uri.Equals(subcategoryUri));
        }

        public async Task<IEnumerable<AdminSubcategoriesDto>> GetSubcategoriesForAdmin()
        {
            var categories = await GetOrderedCategories();

            var output = new List<AdminSubcategoriesDto>();

            foreach (var category in categories)
            {
                var list = new List<AdminSubcategoryDto>();
                foreach (var subcategory in category.Subcategories)
                {
                    var numberOfProducts = await _context.Products.CountAsync(p => p.SubcategoryId == subcategory.Id);
                    list.Add(AdminSubcategoryDto.Create(subcategory, numberOfProducts));
                }

                output.Add(AdminSubcategoriesDto.Create(category, list));
            }

            return output;
        }

        public async Task<IEnumerable<Subcategory>> GetSubcategories()
        {
            return await _context.Subcategories.OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<Subcategory> GetSubcategory(int id)
        {
            return await _context.Subcategories.FindAsync(id);
        }

        public async Task<int> AddSubcategory(int categoryId, string name)
        {
            var category = await _context.Categories.FindAsync(categoryId);

            if (category == null)
            {
                throw new KeyNotFoundException("Category not found");
            }

            if (await DoesSubcategoryExist(name))
            {
                throw new ArgumentException("This name already exists");
            }

            var subcategory = new Subcategory
            {
                Uri = name.GetUniqueUri(false),
                Name = name,
                CategoryId = categoryId
            };

            await Do(async () => await _context.Subcategories.AddAsync(subcategory));

            return subcategory.Id;
        }

        public async Task<int> UpdateSubcategory(int id, int categoryId, string name)
        {
            var subcategory = await _context.Subcategories.FindAsync(id);

            if (subcategory == null)
            {
                throw new KeyNotFoundException("Subcategory not found");
            }

            var category = await _context.Categories.FindAsync(categoryId);

            if (category == null)
            {
                throw new KeyNotFoundException("Category not found");
            }

            if (await DoesSubcategoryExist(name))
            {
                throw new ArgumentException("This name already exists");
            }

            await Do(() =>
            {
                subcategory.Uri = name.GetUniqueUri(false);
                subcategory.Name = name;
                subcategory.CategoryId = categoryId;
                _context.Entry(subcategory).State = EntityState.Modified;
            });

            return subcategory.Id;
        }

        public async Task DeleteSubcategory(int id)
        {
            var subcategory = await _context.Subcategories.FindAsync(id);

            if (subcategory == null)
            {
                throw new KeyNotFoundException("Subcategory not found");
            }

            await Do(() =>
            {
                _context.Remove(subcategory);
            });
        }

        private async Task<List<Category>> GetOrderedCategories()
        {
            return await _context.Categories.Include(c => c.Subcategories).OrderBy(c => c.Name).ToListAsync();
        }

        private async Task<bool> DoesCategoryExist(string categoryName)
        {
            return await _context.Categories.AnyAsync(s => s.Name.Equals(categoryName.Trim()));
        }

        private async Task<bool> DoesSubcategoryExist(string subcategoryName)
        {
            return await _context.Subcategories.AnyAsync(s => s.Name.Equals(subcategoryName.Trim()));
        }
    }
}
