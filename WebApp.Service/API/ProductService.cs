using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Data.Entities;
using WebApp.Service.Common;
using WebApp.Service.Enums;
using WebApp.Service.ResultModels;
using WebApp.Shared.Requests;
using WebApp.Shared.Responses;

namespace AppWeb.Service.API
{
    public class ProductService
    {
        private readonly WebAppContext _context;
        private readonly CommonMethod _commonMethod;

        public ProductService(WebAppContext context, CommonMethod commonMethod)
        {
            _context = context;
            _commonMethod = commonMethod;
        }

        public async Task<IActionResult> GetAllProductAsync()
        {
            var action = new Func<Task<IActionResult>>(async () =>
            {
                var apiResult = new ApiResult<List<ProductResponse>>();

                var products = await _context.Products.Select(p => new ProductResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    CreateAdd = p.CreateAdd,
                    CategoryName = p.Category != null ? p.Category.Name : string.Empty,
                }).ToListAsync();

                apiResult.StatusCode = ApiStatusCode.OK;
                apiResult.Data = products;
                apiResult.Message = "Products retrieved successfully";

                var result = _commonMethod.HandleApiResultAsync<List<ProductResponse>>(apiResult);
                return result;
            });

            return await ErrorHandler.ExecuteAsync(action);
        }

        public async Task<IActionResult> GetProductByIdAsync(string id)
        {
            var action = new Func<Task<IActionResult>>(async () =>
            {
                var apiResult = new ApiResult<Product>();
                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    apiResult.StatusCode = ApiStatusCode.NotFound;
                    apiResult.Message = "Product not found";
                }
                else
                {
                    apiResult.StatusCode = ApiStatusCode.OK;
                    apiResult.Data = product;
                }

                var result = _commonMethod.HandleApiResultAsync<Product>(apiResult);
                return result;
            });

            return await ErrorHandler.ExecuteAsync(action);
        }

        public async Task<IActionResult> CreateProductAsync(ProductRequest productRequest)
        {
            var action = new Func<Task<IActionResult>>(async () =>
            {
                var apiResult = new ApiResult<ProductResponse>();

                if (productRequest == null)
                {
                    apiResult.StatusCode = ApiStatusCode.BadRequest;
                    apiResult.Message = "Product cannot be null";
                }
                else
                {
                    var categoryExists = await _context.CategoryProducts.AnyAsync(c => c.Id == productRequest.CategoryId);
                    if (!categoryExists)
                    {
                        apiResult.StatusCode = ApiStatusCode.BadRequest;
                        apiResult.Message = "CategoryId does not exist";
                    }
                    else
                    {
                        var product = new Product
                        {
                            Id = Guid.NewGuid().ToString(),
                            Name = productRequest.Name,
                            CreateAdd = productRequest.CreateAdd,
                            CategoryId = productRequest.CategoryId
                        };


                        _context.Products.Add(product);
                        await _context.SaveChangesAsync();

                        var productResponse = new ProductResponse
                        {
                            Id = product.Id,
                            Name = product.Name,
                            CreateAdd = product.CreateAdd,
                            CategoryId = product.CategoryId
                        };

                        apiResult.StatusCode = ApiStatusCode.Created;
                        apiResult.Message = "Product created successfully";
                        apiResult.Data = productResponse;
                    }
                }
                var result = _commonMethod.HandleApiResultAsync<ProductResponse>(apiResult);
                return result;
            });

            return await ErrorHandler.ExecuteAsync(action);
        }

        public async Task<IActionResult> UpdateProductAsync(string id, ProductRequest productRequest)
        {
            var action = new Func<Task<IActionResult>>(async () =>
            {
                var apiResult = new ApiResult<string>();
                if (string.IsNullOrEmpty(id) || productRequest == null || id != productRequest.Id)
                {
                    apiResult.StatusCode = ApiStatusCode.BadRequest;
                    apiResult.Message = "Invalid product ID or request data";
                }
                else
                {
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

                        _context.Products.Update(product);
                        await _context.SaveChangesAsync();

                        apiResult.StatusCode = ApiStatusCode.OK;
                        apiResult.Message = "Product updated successfully";
                    }
                }

                return _commonMethod.HandleApiResultAsync(apiResult);
            });

            return await ErrorHandler.ExecuteAsync(action);
        }

        public async Task<IActionResult> DeleteProductAsync(string? id)
        {
            var action = new Func<Task<IActionResult>>(async () =>
            {
                var apiResult = new ApiResult<string>();

                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    apiResult.StatusCode = ApiStatusCode.NotFound;
                    apiResult.Message = "Product not found";
                }
                else
                {
                    _context.Products.Remove(product);
                    await _context.SaveChangesAsync();

                    apiResult.StatusCode = ApiStatusCode.OK;
                    apiResult.Message = "Product deleted successfully";
                }

                return _commonMethod.HandleApiResultAsync(apiResult);
            });

            return await ErrorHandler.ExecuteAsync(action);
        }
    }
}
