using System.Threading.Tasks;
using Markt.Dtos;
using Markt.Helpers;
using Markt.Helpers.Attributes.Filterable;
using Markt.Helpers.Attributes.Searchable;
using Markt.Models;
using Markt.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Markt.Controllers
{
    public class ProductsController : ApiHelper
    {
        private readonly IProductService _productService;
        private readonly IBrandService _brandService;

        public ProductsController(IProductService productService, IBrandService brandService)
        {
            _productService = productService;
            _brandService = brandService;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("Home")]
        public IActionResult GetHomeProducts([FromQuery] SearchOptions<ProductResultDto, Product> searchOptions, [FromQuery] PagingOptions pagingOptions)
        {
            return Do(() => _productService.GetProductsForHome(searchOptions, pagingOptions));
        }

        /// <summary>
        /// The main subcategory products page
        /// </summary>
        /// <param name="subcategoryUri">The subcategory uri</param>
        /// <param name="brands">Brand filters by names separated with |</param>
        /// <param name="searchOptions">Contains the search query</param>
        /// <param name="pagingOptions">Contains the offset and limit properties</param>
        [HttpGet]
        [AllowAnonymous]
        [Route("Subcategory/{subcategoryUri}/{brands}")]
        public async Task<IActionResult> GetSubcategoryProducts(string subcategoryUri, string brands,
            [FromQuery] SearchOptions<ProductResultDto, Product> searchOptions, [FromQuery] PagingOptions pagingOptions)
        {
            if (subcategoryUri == null || subcategoryUri.Trim().Equals(string.Empty))
            {
                return BadRequest("Invalid subcategory uri");
            }

            var filterOptions = brands != null && !brands.Equals(string.Empty) ?
                new FilterOptions<ProductResultDto, Product>(await _brandService.GetBrandIds(brands.Split('|'))) :
                null;

            return await Do(async () =>
                await _productService.GetProductsBySubcategory(subcategoryUri, filterOptions, searchOptions,
                    pagingOptions));
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("{productUri}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductByUri(string productUri)
        {
            string userId;

            try
            {
                userId = User.Identity.GetUserId();
            }
            catch
            {
                userId = null;
            }

            return await Do(async () => await _productService.GetSingleProduct(userId, productUri));
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("Cart")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductById(int productId)
        {
            return await Do(async () => await _productService.GetProductForCart(productId));
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("Seller/{username}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSellerProducts(string username)
        {
            return await Do(async () => await _productService.GetSellerProducts(username));
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("Brands")]
        public async Task<IActionResult> GetAllBrands(string subcategoryUri = null)
        {
            return await Do(async () => await _brandService.GetAll(subcategoryUri));
        }

        [HttpPut]
        [Authorize]
        [Route("Review")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateReview(int reviewId, int rate)
        {
            return await Do(async () => await _productService.UpdateReview(User.Identity.GetUserId(), reviewId, rate));
        }

        [HttpPost]
        [Route("Review")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddReview(int productId, int rate)
        {
            return await Create(nameof(GetProductByUri),
                async () => await _productService.AddReview(User.Identity.GetUserId(), productId, rate));
        }

        [HttpGet]
        [Route("Brands/Admin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(Policy = nameof(UserRole.Admin))]
        public async Task<IActionResult> GetAdminBrands(int id = 0)
        {
            if (id == 0)
            {
                return await Do(async () => await _brandService.GetAdminBrands());
            }

            return await Do(async () => await _brandService.GetBrand(id));
        }

        [HttpPost]
        [Route("Brands/Admin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(Policy = nameof(UserRole.Admin))]
        public async Task<IActionResult> AddBrand([FromBody] BrandDto brandDto)
        {
            return await Create(nameof(GetAdminBrands), async () => await _brandService.AddBrand(brandDto.Name));
        }

        [HttpPut]
        [Route("Brands/Admin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(Policy = nameof(UserRole.Admin))]
        public async Task<IActionResult> UpdateBrand(int id, [FromBody] BrandDto brandDto)
        {
            return await Do(async () => await _brandService.UpdateBrand(id, brandDto.Name));
        }

        [HttpDelete]
        [Route("Brands/Admin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(Policy = nameof(UserRole.Admin))]
        public async Task<IActionResult> DeleteBrand(int id)
        {
            return await Do(async () => await _brandService.DeleteBrand(id));
        }

        [HttpGet]
        [Route("Reports")]
        [Authorize(Policy = nameof(UserRole.Seller))]
        public async Task<IActionResult> GetReports()
        {
            return await Do(async () => await _productService.GetReports(User.Identity.GetUserId()));
        }

        [HttpGet]
        [Authorize(Policy = nameof(UserRole.Seller))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProducts(int id = 0)
        {
            if (id == 0)
            {
                return await Do(async () => await _productService.GetUserProducts(User.Identity.GetUserId()));
            }

            return await Do(async () => await _productService.GetProduct(User.Identity.GetUserId(), id));
        }

        [HttpPost]
        [Authorize(Policy = nameof(UserRole.Seller))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddProduct([FromForm] ProductDto productDto)
        {
            return await Create(nameof(GetProductById),
                async () => await _productService.AddProduct(User.Identity.GetUserId(), productDto));
        }

        [HttpPut]
        [Authorize(Policy = nameof(UserRole.Seller))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] ProductDto productDto)
        {
            return await Do(async () => await _productService.UpdateProduct(User.Identity.GetUserId(), id, productDto));
        }

        [HttpDelete]
        [Route("Images")]
        [Authorize(Policy = nameof(UserRole.Seller))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProductImage(int productId, int imageId)
        {
            return await Do(async () => await _productService.DeleteImage(User.Identity.GetUserId(), productId, imageId));
        }

        [HttpDelete]
        [Authorize(Policy = nameof(UserRole.Seller))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            return await Do(async () => await _productService.DeleteProduct(User.Identity.GetUserId(), id));
        }
    }
}