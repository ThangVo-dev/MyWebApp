using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Data.Entities;
using WebApp.Service.API;
using WebApp.Shared.Models.Category;

namespace WebApp.API.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("api/categoryproduct")]
    [ApiController]
    public class CategoryProductController : ControllerBase
    {
        private readonly WebAppContext _context;
        private readonly CategoryProdService _categoryProdService;

        public CategoryProductController(WebAppContext context, CategoryProdService categoryProdService)
        {
            _categoryProdService = categoryProdService;
            _context = context;
        }

        // GET: api/CategoryProduct
        [HttpGet]
        [Route("get-all")]
        public async Task<IActionResult> GetCategoryProducts()
        {
            var categoryProducts = await _categoryProdService.GetAllAsync();
            return categoryProducts;
        }

        // GET: api/CategoryProduct/5
        [HttpGet]
        [Route("get/{id}")]
        public async Task<IActionResult> GetCategoryProduct(string id)
        {
            var categoryProduct = await _categoryProdService.GetByIdAsync(id);
            return categoryProduct;
        }

        // POST: api/CategoryProduct
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreateCategoryProduct(CategoryProductMdl categoryProduct)
        {
            var result = await _categoryProdService.CreateAsync(categoryProduct);
            return result;
        }

        // PUT: api/CategoryProduct/5
        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> UpdateCategoryProduct(CategoryProductMdl categoryProduct)
        {
            var result = await _categoryProdService.UpdateAsync(categoryProduct);
            return result;
        }

        // DELETE: api/CategoryProduct/5
        [HttpDelete]
        [Route("delete/{id}")]
        public async Task<IActionResult> DeleteCategoryProduct(string id)
        {
            var result = await _categoryProdService.DeleteAsync(id);
            return result;
        }
    }
}
