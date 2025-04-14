using AppWeb.Service.API;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Data.Entities;
using WebApp.Service.Enums;
using WebApp.Shared.Requests;
using WebApp.Shared.Responses;

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
        public async Task<ActionResult<IEnumerable<ProductResponse>>> GetProducts()
        {
            // return await _context.Products.ToListAsync();
            var productService = await _productService.GetProductsAsync();
            return productService;
        }

        // GET: api/Product/5
        [HttpGet]
        [Route("get/{id}")]
        public async Task<ActionResult<Product>> GetProduct(string id)
        {
            var productService = await _productService.GetProductByIdAsync(id);
            if(productService.StatusCode == ApiStatusCode.NotFound)
            {
                return NotFound(new { Message = productService.Message });
            }
            return Ok(productService.Data);
        }

        // POST: api/Product
        [HttpPost]
        [Route("create")]
        public async Task<ActionResult<Product>> CreateProduct(ProductRequest productRequest)
        {
            var productService = await _productService.CreateProductAsync(productRequest);
            if (productService.StatusCode == ApiStatusCode.BadRequest)
            {
                return BadRequest(new { Message = productService.Message });
            }

            return CreatedAtAction(nameof(GetProduct), new { id = productService?.Data?.Id }, productService?.Data);
        }

        // PUT: api/Product/5
        [HttpPut]
        [Route("update/{id}")]
        public async Task<IActionResult> UpdateProduct(string id, ProductRequest productRequest)
        {
            var productService = await _productService.UpdateProductAsync(id, productRequest);
            if (productService.StatusCode == ApiStatusCode.NotFound)
            {
                return NotFound(new { Message = productService.Message });
            }
            else if (productService.StatusCode == ApiStatusCode.BadRequest)
            {
                return BadRequest(new { Message = productService.Message });
            }

            return Ok(new { Message = productService.Message });
        }

        // DELETE: api/Product/5
        [HttpDelete]
        [Route("delete/{id}")]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductExists(string id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
