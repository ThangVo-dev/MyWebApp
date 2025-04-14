using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Data.Entities;

namespace WebApp.API.Controllers
{
    [Authorize]
    [Route("api/categoryproduct")]
    [ApiController]
    public class CategoryProductController : ControllerBase
    {
        private readonly WebAppContext _context;

        public CategoryProductController(WebAppContext context)
        {
            _context = context;
        }

        // GET: api/CategoryProduct
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryProduct>>> GetCategoryProducts()
        {
            return await _context.CategoryProducts.ToListAsync();
        }

        // GET: api/CategoryProduct/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryProduct>> GetCategoryProduct(string id)
        {
            var categoryProduct = await _context.CategoryProducts.FirstOrDefaultAsync(c => c.Id == id);

            if (categoryProduct == null)
            {
                return NotFound();
            }

            return categoryProduct;
        }

        // POST: api/CategoryProduct
        [HttpPost]
        [Route("Create")]
        public async Task<ActionResult<CategoryProduct>> CreateCategoryProduct(CategoryProduct categoryProduct)
        {
            if (categoryProduct == null)
            {
                return BadRequest();
            }

            _context.CategoryProducts.Add(categoryProduct);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategoryProduct), new { id = categoryProduct.Id }, categoryProduct);
        }

        // PUT: api/CategoryProduct/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategoryProduct(string id, CategoryProduct categoryProduct)
        {
            if (id != categoryProduct.Id)
            {
                return BadRequest();
            }

            _context.Entry(categoryProduct).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/CategoryProduct/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategoryProduct(string id)
        {
            var categoryProduct = await _context.CategoryProducts.FindAsync(id);
            if (categoryProduct == null)
            {
                return NotFound();
            }

            _context.CategoryProducts.Remove(categoryProduct);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CategoryProductExists(string id)
        {
            return _context.CategoryProducts.Any(e => e.Id == id);
        }
    }
}
