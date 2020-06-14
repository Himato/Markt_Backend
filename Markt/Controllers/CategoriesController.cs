using System.Threading.Tasks;
using Markt.Dtos;
using Markt.Helpers;
using Markt.Models;
using Markt.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Markt.Controllers
{
    [Authorize(Policy = nameof(UserRole.Admin))]
    public class CategoriesController : ApiHelper
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategories()
        {
            return await Do(async () => await _categoryService.GetAll());
        }

        [HttpGet]
        [Route("Admin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> GetCategoriesForAdmin(int id = 0)
        {
            if (id == 0)
            {
                return await Do(async () => await _categoryService.GetCategoriesForAdmin());
            }

            return await Do(async () => await _categoryService.GetCategory(id));
        }

        [HttpPost]
        [Route("Admin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> AddCategory([FromBody] CategoryDto categoryDto)
        {
            return await Create(nameof(GetCategoriesForAdmin), async () => await _categoryService.AddCategory(categoryDto.Name));
        }

        [HttpPut]
        [Route("Admin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryDto categoryDto)
        {
            return await Do(async () => await _categoryService.UpdateCategory(id, categoryDto.Name));
        }

        [HttpDelete]
        [Route("Admin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            return await Do(async () => await _categoryService.DeleteCategory(id));
        }

        [HttpGet]
        [Route("Subcategories")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSubcategories()
        {
            return await Do(async () => await _categoryService.GetSubcategories());
        }

        [HttpGet]
        [Route("Subcategories/Admin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> GetSubcategoriesForAdmin(int id = 0)
        {
            if (id == 0)
            {
                return await Do(async () => await _categoryService.GetSubcategoriesForAdmin());
            }

            return await Do(async () => await _categoryService.GetSubcategory(id));
        }

        [HttpPost]
        [Route("Subcategories/Admin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> AddSubcategory([FromBody] SubcategoryDto subcategoryDto)
        {
            return await Create(nameof(GetSubcategoriesForAdmin),
                async () => await _categoryService.AddSubcategory(subcategoryDto.CategoryId, subcategoryDto.Name));
        }

        [HttpPut]
        [Route("Subcategories/Admin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> UpdateSubcategory(int id, [FromBody] SubcategoryDto subcategoryDto)
        {
            return await Do(async () => await _categoryService.UpdateSubcategory(id, subcategoryDto.CategoryId, subcategoryDto.Name));
        }

        [HttpDelete]
        [Route("Subcategories/Admin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> DeleteSubcategory(int id)
        {
            return await Do(async () => await _categoryService.DeleteSubcategory(id));
        }
    }
}