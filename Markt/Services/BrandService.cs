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
    public interface IBrandService
    {
        Task<string[]> GetBrandIds(string[] brands);

        Task<IEnumerable<Brand>> GetAll(string subcategoryUri = null);

        Task<IEnumerable<AdminBrandDto>> GetAdminBrands();

        Task<Brand> GetBrand(int id);

        Task<bool> IsBrand(int id);

        Task<int> AddBrand(string name);

        Task UpdateBrand(int id, string name);

        Task DeleteBrand(int id);
    }

    public class BrandService : ServiceHelper, IBrandService
    {
        private readonly ApplicationDbContext _context;

        public BrandService(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<string[]> GetBrandIds(string[] brands)
        {
            return await _context.Brands
                .Where(b => brands.Any(c => c.Equals(b.Name, StringComparison.OrdinalIgnoreCase)))
                .Select(b => b.Id.ToString()).ToArrayAsync();
        }

        public async Task<IEnumerable<Brand>> GetAll(string subcategoryUri = null)
        {
            if (subcategoryUri == null)
            {
                return await _context.Brands.OrderBy(b => b.Name).ToListAsync();
            }

            var subcategory = await _context.Subcategories.FirstOrDefaultAsync(s => s.Uri.Equals(subcategoryUri));

            if (subcategory == null)
            {
                return await _context.Brands.OrderBy(b => b.Name).ToListAsync();
            }

            return await _context.Products
                .Where(p => p.SubcategoryId == subcategory.Id)
                .Include(p => p.Brand)
                .Select(p => p.Brand)
                .Distinct()
                .OrderBy(b => b.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<AdminBrandDto>> GetAdminBrands()
        {
            var brands = await _context.Brands.ToListAsync();

            var output = new List<AdminBrandDto>();

            foreach (var brand in brands)
            {
                var numberOfProducts = await _context.Products.CountAsync(p => p.BrandId == brand.Id);
                output.Add(AdminBrandDto.Create(brand, numberOfProducts));
            }

            return output;
        }

        public async Task<Brand> GetBrand(int id)
        {
            return await _context.Brands.FindAsync(id);
        }

        public async Task<bool> IsBrand(int id)
        {
            var brand = await GetBrand(id);

            return brand != null;
        }

        public async Task<int> AddBrand(string name)
        {
            if (await DoesExist(name))
            {
                throw new ArgumentException("This name already exists");
            }

            var brand = new Brand
            {
                Name = name
            };

            await Do(async () => await _context.Brands.AddAsync(brand));

            return brand.Id;
        }

        public async Task UpdateBrand(int id, string name)
        {
            var brand = await GetBrand(id);

            if (brand == null)
            {
                throw new KeyNotFoundException("Brand not found");
            }

            if (await DoesExist(name))
            {
                throw new ArgumentException("This name already exists");
            }

            brand.Name = name;

            await Do(() => _context.Entry(brand).State = EntityState.Modified);
        }

        public async Task DeleteBrand(int id)
        {
            var brand = await GetBrand(id);

            if (brand == null)
            {
                throw new KeyNotFoundException("Brand not found");
            }

            if (await _context.Products.AnyAsync(p => p.BrandId == id))
            {
                throw new ArgumentException("Can't delete a brand with products");
            }

            await Do(() => _context.Brands.Remove(brand));
        }

        private async Task<bool> DoesExist(string name)
        {
            return await _context.Brands.AnyAsync(b => b.Name.Equals(name.Trim()));
        }
    }
}
