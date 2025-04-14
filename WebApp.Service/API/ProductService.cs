using Microsoft.EntityFrameworkCore;
using WebApp.Data.Entities;
using WebApp.Service.Enums;
using WebApp.Service.ResultModels;
using WebApp.Shared.Requests;
using WebApp.Shared.Responses;

namespace AppWeb.Service.API
{
    public class ProductService
    {
        private readonly WebAppContext _context;

        public ProductService(WebAppContext context)
        {
            _context = context;
        }

        public async Task<List<ProductResponse>> GetProductsAsync()
        {
            var products = await _context.Products.Select(p => new ProductResponse
            {
                Id = p.Id,
                Name = p.Name,
                CreateAdd = p.CreateAdd,
                CategoryName = p.Category.Name,
            }).ToListAsync();

            return products;
        }

        public async Task<ApiResult<Product>> GetProductByIdAsync(string id)
        {
            var apiResult = new ApiResult<Product>();
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                apiResult.StatusCode = ApiStatusCode.NotFound;
                apiResult.Message = "Product not found";
                apiResult.Data = null;
                return apiResult;
            }

            apiResult.StatusCode = ApiStatusCode.OK;
            apiResult.Data = product;
            return apiResult;
        }

        public async Task<ApiResult<ProductResponse>> CreateProductAsync(ProductRequest productRequest)
        {
            var apiResult = new ApiResult<ProductResponse>();

            if (productRequest == null)
            {
                apiResult.StatusCode = ApiStatusCode.BadRequest;
                apiResult.Message = "Product cannot be null";
                return apiResult;
            }

            var categoryExists = await _context.CategoryProducts.AnyAsync(c => c.Id == productRequest.CategoryId);
            if (!categoryExists)
            {
                apiResult.StatusCode = ApiStatusCode.BadRequest;
                apiResult.Message = "CategoryId does not exist";
                return apiResult;
            }

            var product = new Product
            {
                Id = Guid.NewGuid().ToString(),
                Name = productRequest.Name,
                CreateAdd = productRequest.CreateAdd,
                CategoryId = productRequest.CategoryId
            };

            var productResponse = new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                CreateAdd = product.CreateAdd,
                CategoryId = product.CategoryId
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            apiResult.StatusCode = ApiStatusCode.Created;
            apiResult.Message = "Product created successfully";
            apiResult.Data = productResponse;
            return apiResult;
        }

        public async Task<ApiResult<string>> UpdateProductAsync(string id, ProductRequest productRequest)
        {
            var apiResult = new ApiResult<string>();
            if (id != productRequest.Id)
            {
                apiResult.StatusCode = ApiStatusCode.BadRequest;
                apiResult.Message = "Product ID mismatch";
                // return apiResult;
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                apiResult.StatusCode = ApiStatusCode.NotFound;
                apiResult.Message = "Product not found";
            }
            else
            {
                product.Name = productRequest.Name;
                product.CreateAdd = productRequest.CreateAdd;
                product.CategoryId = productRequest.CategoryId;
                _context.Entry(product).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(id))
                    {
                        apiResult.StatusCode = ApiStatusCode.NotFound;
                        apiResult.Message = "Product not found";
                        // return apiResult;
                    }
                    else
                    {
                        throw;
                    }
                }

                apiResult.StatusCode = ApiStatusCode.OK;
                apiResult.Message = "Product updated successfully";
            }
            return apiResult;
        }

        private bool ProductExists(string id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
