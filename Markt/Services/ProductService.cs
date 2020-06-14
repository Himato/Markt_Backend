using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Markt.Dtos;
using Markt.Helpers;
using Markt.Helpers.Attributes.Filterable;
using Markt.Helpers.Attributes.Searchable;
using Markt.Models;
using Microsoft.EntityFrameworkCore;

namespace Markt.Services
{
    public interface IProductService
    {
        IEnumerable<ProductResultDto> GetProductsForHome(SearchOptions<ProductResultDto, Product> searchOptions,
            PagingOptions pageOptions);

        Task<IEnumerable<ProductResultDto>> GetProductsBySubcategory(string subcategoryUri, FilterOptions<ProductResultDto, Product> filterOptions,
            SearchOptions<ProductResultDto, Product> searchOptions, PagingOptions pageOptions);

        Task<SingleProductDto> GetSingleProduct(string userId, string productUri);

        Task<ProductResultDto> GetProductForCart(int productId);

        Task<SellerProductsDto> GetSellerProducts(string sellerUsername);

        Task<bool> IsProduct(int id);

        /* All of the following methods for the seller account */
        Task<IEnumerable<ReportDto>> GetReports(string userId);

        Task<IEnumerable<Product>> GetUserProducts(string userId);

        Task<Product> GetProduct(string userId, int id);

        Task<int> AddReview(string userId, int productId, int rate);

        Task UpdateReview(string userId, int reviewId, int rate);

        /// <returns>Product Uri</returns>
        Task<int> AddProduct(string userId, ProductDto productDto);
        
        /// <returns>Product Uri</returns>
        Task<int> UpdateProduct(string userId, int id, ProductDto productDto);

        Task DeleteImage(string userId, int productId, int imageId);
        
        Task DeleteProduct(string userId, int id);
    }

    public class ProductService : ServiceHelper, IProductService
    {
        private readonly IUserService _userService;
        private readonly ICategoryService _categoryService;
        private readonly IBrandService _brandService;
        private readonly IImageService _imageService;
        private readonly ApplicationDbContext _context;

        public ProductService(IUserService userService, ICategoryService categoryService,
            IBrandService brandService, IImageService imageService, ApplicationDbContext context) : base(context)
        {
            _userService = userService;
            _categoryService = categoryService;
            _brandService = brandService;
            _imageService = imageService;
            _context = context;
        }

        public IEnumerable<ProductResultDto> GetProductsForHome(SearchOptions<ProductResultDto, Product> searchOptions, PagingOptions pageOptions)
        {
            if (pageOptions == null)
            {
                pageOptions = new PagingOptions
                {
                    Offset = 0,
                    Limit = 25
                };
            }

            var query = _context.Products.AsQueryable();

            return searchOptions.Apply(query)
                .Include(p => p.Images)
                .Include(p => p.Seller)
                .Skip(pageOptions.Offset ?? default)
                .Take(pageOptions.Limit ?? 25).AsEnumerable()
                .Select(ProductResultDto.Create);
        }

        public async Task<IEnumerable<ProductResultDto>> GetProductsBySubcategory(string subcategoryUri, FilterOptions<ProductResultDto, Product> filterOptions,
            SearchOptions<ProductResultDto, Product> searchOptions, PagingOptions pageOptions)
        {
            var subcategory = await _categoryService.GetSubcategoryByUri(subcategoryUri);

            if (subcategory == null)
            {
                throw new KeyNotFoundException("Subcategory not found");
            }

            var products = _context.Products
                .Where(p => p.SubcategoryId == subcategory.Id).AsQueryable();

            if (!products.Any()) return new List<ProductResultDto>();

            if (pageOptions == null)
            {
                pageOptions = new PagingOptions
                {
                    Offset = 0,
                    Limit = 25
                };
            }

            IQueryable<Product> query = null;

            if (filterOptions != null)
            {
                 query = filterOptions.Apply(products);

                if (query == null)
                {
                    return new List<ProductResultDto>();
                }
            }

            return searchOptions.Apply(query ?? products)
                .Include(p => p.Images)
                .Include(p => p.Seller)
                .Skip(pageOptions.Offset ?? default)
                .Take(pageOptions.Limit ?? 25).AsEnumerable()
                .Select(ProductResultDto.Create);
        }

        public async Task<SingleProductDto> GetSingleProduct(string userId, string productUri)
        {
            var product = await _context.Products
                .Where(p => p.Uri.Equals(productUri))
                .Include(p => p.Images)
                .Include(p => p.Reviews)
                .Include(p => p.Seller)
                .FirstOrDefaultAsync();

            if (product == null)
            {
                throw new KeyNotFoundException("Product not found");
            }

            return SingleProductDto.Create(userId, product);
        }

        public async Task<ProductResultDto> GetProductForCart(int productId)
        {
            var product = await _context.Products
                .Include(p => p.Seller)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                throw new KeyNotFoundException("Product not found");
            }

            return ProductResultDto.Create(product);
        }

        public async Task<SellerProductsDto> GetSellerProducts(string sellerUsername)
        {
            var seller = await _userService.GetUserByUsername(sellerUsername);

            if (seller == null)
            {
                throw new KeyNotFoundException("Seller not found");
            }

            return new SellerProductsDto
            {
                SellerName = seller.GetName(),
                Products = _context.Products
                    .Where(p => p.SellerId.Equals(seller.Id))
                    .Include(p => p.Images)
                    .Include(p => p.Seller)
                    .Select(ProductResultDto.Create)
            };
        }

        public async Task<bool> IsProduct(int id)
        {
            return await _context.Products.AnyAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<ReportDto>> GetReports(string userId)
        {
            var products = await _context.Products.Where(p => p.SellerId.Equals(userId)).ToListAsync();

            var output = new List<ReportDto>();

            foreach (var product in products)
            {
                var purchases = await _context.Purchases.Where(p => p.ProductId == product.Id).ToListAsync();
                output.Add(ReportDto.Create(product, purchases));
            }

            return output;
        }

        public async Task<IEnumerable<Product>> GetUserProducts(string userId)
        {
            return await _context.Products
                .Where(p => p.SellerId.Equals(userId))
                .OrderByDescending(p => p.DateTime)
                .Include(p => p.Subcategory)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .ToListAsync();
        }

        public async Task<Product> GetProduct(string userId, int id)
        {
            var product = await _context.Products
                .Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                throw new KeyNotFoundException("Product not found");
            }

            if (!product.SellerId.Equals(userId))
            {
                throw new AuthenticationException();
            }

            return product;
        }

        public async Task<int> AddReview(string userId, int productId, int rate)
        {
            var review =
                await _context.Reviews.FirstOrDefaultAsync(r =>
                    r.UserId.Equals(userId) && r.ProductId == productId);

            if (review != null)
            {
                throw new ArgumentException("You've already reviewed this product");
            }

            if (!await _context.Purchases.AnyAsync(p => p.UserId.Equals(userId) && p.ProductId == productId && p.IsFinished))
            {
                throw new ArgumentException("You haven't purchase this product yet");
            }

            review = new Review
            {
                Rate = rate,
                ProductId = productId,
                UserId = userId
            };

            await Do(async () => await _context.Reviews.AddAsync(review));

            return review.Id;
        }

        public async Task UpdateReview(string userId, int reviewId, int rate)
        {
            var review = await _context.Reviews.FindAsync(reviewId);

            if (review == null || !review.UserId.Equals(userId))
            {
                throw new KeyNotFoundException("Review not found");
            }

            if (rate < 0 || rate > 5)
            {
                throw new ArgumentException("Invalid rate value");
            }

            review.Rate = rate;

            await Do(() => _context.Entry(review).State = EntityState.Modified);
        }

        public async Task<int> AddProduct(string userId, ProductDto productDto)
        {
            var user = await _userService.GetUserById(userId);

            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            var subcategory = await _categoryService.GetSubcategory(productDto.SubcategoryId);

            if (subcategory == null)
            {
                throw new KeyNotFoundException("Subcategory not found");
            }

            if (!await _brandService.IsBrand(productDto.BrandId))
            {
                throw new KeyNotFoundException("Brand not found");
            }

            if (productDto.Images == null || !productDto.Images.Any())
            {
                throw new ArgumentException("You've to provide at least one image");
            }

            var product = new Product
            {
                Uri = productDto.Name.GetUniqueUri(true),
                Name = productDto.Name,
                Description = productDto.Description,
                Specification = productDto.Specification,
                ReturnInfo = productDto.ReturnInfo,
                DateTime = DateTime.UtcNow,
                Price = productDto.Price,
                IsInStock = productDto.IsInStock,
                SubcategoryId = subcategory.Id,
                BrandId = productDto.BrandId,
                SellerId = user.Id
            };

            await Do(async () => await _context.Products.AddAsync(product));

            var images = new List<string>();

            foreach (var image in productDto.Images)
            {
                var result = await _imageService.UploadImage(product.Id, image);

                if (result == null) continue;

                images.Add(result);
            }

            if (!images.Any())
            {
                throw new ArgumentException("Couldn't upload any images, provide valid images");
            }

            return product.Id;
        }

        public async Task<int> UpdateProduct(string userId, int id, ProductDto productDto)
        {
            var product = await GetProduct(userId, id);

            var subcategory = await _categoryService.GetSubcategory(productDto.SubcategoryId);

            if (subcategory == null)
            {
                productDto.SubcategoryId = product.SubcategoryId;
            }

            if (!await _brandService.IsBrand(productDto.BrandId))
            {
                throw new KeyNotFoundException("Brand not found");
            }

            product.Uri = productDto.Name.GetUniqueUri(true);
            product.Name = productDto.Name;
            product.Description = productDto.Description;
            product.Specification = productDto.Specification;
            product.ReturnInfo = productDto.ReturnInfo;
            product.Price = productDto.Price;
            product.IsInStock = productDto.IsInStock;
            product.SubcategoryId = productDto.SubcategoryId;
            product.BrandId = productDto.BrandId;

            await Do(() => _context.Entry(product).State = EntityState.Modified);

            foreach (var image in productDto.Images)
            {
                await _imageService.UploadImage(product.Id, image);
            }

            return product.Id;
        }

        public async Task DeleteImage(string userId, int productId, int imageId)
        {
            await GetProduct(userId, productId);

            var images = await _imageService.GetProductImages(productId);

            if (images.Count() <= 1)
            {
                throw new ArgumentException("Can't delete the only image the product has");
            }

            await _imageService.DeleteImage(imageId, productId);
        }

        public async Task DeleteProduct(string userId, int id)
        {
            var product = await GetProduct(userId, id);

            if (await _context.Purchases.AnyAsync(p => p.ProductId == id && !p.IsFinished))
            {
                throw new ArgumentException("Can't delete a product while there is a running purchases");
            }

            await Do(() => _context.Products.Remove(product));

            var images = await _imageService.GetProductImages(id);

            foreach (var image in images)
            {
                await _imageService.DeleteImage(image.Id, id);
            }
        }
    }
}
