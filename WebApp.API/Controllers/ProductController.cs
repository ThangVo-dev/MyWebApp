using AppWeb.Service.API;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Data.Entities;
using WebApp.Shared.Requests;

namespace WebApp.API.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    [Route("api/product")]
    public class ProductController : ControllerBase
    {
        private readonly WebAppContext _context;
        private ProductService _productService;

        public ProductController(WebAppContext context, ProductService productService)
        {
            _productService = productService;
            _context = context;
        }

        // GET: api/Product
        [HttpGet]
        [Route("get-all")]
        public async Task<IActionResult> GetAllProduct()
        {
            var result = await _productService.GetAllProductAsync();
            return result;
        }

        // GET: api/Product/5
        [HttpGet]
        [Route("get/{id}")]
        public async Task<IActionResult> GetProductById(string id)
        {
            var result = await _productService.GetProductByIdAsync(id);
            return result;
        }

        // POST: api/Product
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreateProduct(ProductRequest productRequest)
        {
            var result = await _productService.CreateProductAsync(productRequest);
            return result;
        }

        // PUT: api/Product/5
        [HttpPut]
        [Route("edit")]
        public async Task<IActionResult> Edit(ProductRequest productRequest)
        {
            var productService = await _productService.UpdateProductAsync(productRequest);
            return productService;
        }

        // DELETE: api/Product/5
        [HttpDelete]
        [Route("delete/{id}")]
        public async Task<IActionResult> DeleteProduct(string? id)
        {
            var productService = await _productService.DeleteProductAsync(id);
            return productService;
        }

    }
}
