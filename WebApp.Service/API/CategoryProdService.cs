using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WebApp.Data.Entities;
using WebApp.Service.Common;
using WebApp.Service.Enums;
using WebApp.Service.ResultModels;
using WebApp.Shared.Models.Category;

namespace WebApp.Service.API
{
    public class CategoryProdService
    {
        private readonly IConfiguration _configuration;
        private readonly WebAppContext _context;
        private readonly CommonMethod _commonMethod;

        public CategoryProdService(IConfiguration configuration, WebAppContext context, CommonMethod commonMethod)
        {
            _context = context;
            _configuration = configuration;
            _commonMethod = commonMethod;
        }

        public async Task<IActionResult> GetAllAsync()
        {
            var action = new Func<Task<IActionResult>>(async () =>
            {
                var apiResult = new ApiResult<List<CategoryProdResponse>>();

                var categoryProducts = await _context.CategoryProducts.Select(c => new CategoryProdResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                }).ToListAsync();

                apiResult.StatusCode = ApiStatusCode.OK;
                apiResult.Data = categoryProducts;
                apiResult.Message = "Category Products retrieved successfully";

                var result = _commonMethod.HandleApiResultAsync<List<CategoryProdResponse>>(apiResult);
                return result;
            });

            return await ErrorHandler.ExecuteAsync(action);
        }

        public async Task<IActionResult> GetByIdAsync(string id)
        {
            var action = new Func<Task<IActionResult>>(async () =>
            {
                var apiResult = new ApiResult<CategoryProduct>();
                var categoryProduct = await _context.CategoryProducts.FirstOrDefaultAsync(c => c.Id == id);

                if (categoryProduct == null)
                {
                    apiResult.StatusCode = ApiStatusCode.NotFound;
                    apiResult.Message = "Category Product not found";
                }
                else
                {
                    apiResult.StatusCode = ApiStatusCode.OK;
                    apiResult.Data = categoryProduct;
                    apiResult.Message = "Category Product retrieved successfully";
                }

                var result = _commonMethod.HandleApiResultAsync<CategoryProduct>(apiResult);
                return result;
            });

            return await ErrorHandler.ExecuteAsync(action);
        }

        public async Task<IActionResult> CreateAsync(CategoryProductMdl categoryProduct)
        {
            var action = new Func<Task<IActionResult>>(async () =>
            {
                var apiResult = new ApiResult<CategoryProductMdl>();

                if (categoryProduct == null)
                {
                    apiResult.StatusCode = ApiStatusCode.BadRequest;
                    apiResult.Message = "Invalid Category Product data";
                }
                else
                {
                    var categoryProductEntity = new CategoryProduct
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = categoryProduct.Name,
                    };

                    _context.CategoryProducts.Add(categoryProductEntity);
                    await _context.SaveChangesAsync();

                    apiResult.StatusCode = ApiStatusCode.Created;
                    apiResult.Data = categoryProduct;
                    apiResult.Message = "Category Product created successfully";
                }

                var commonMethod = new CommonMethod();
                var result = _commonMethod.HandleApiResultAsync<CategoryProductMdl>(apiResult);
                return result;
            });

            return await ErrorHandler.ExecuteAsync(action);
        }

        public async Task<IActionResult> UpdateAsync(CategoryProductMdl categoryProduct)
        {
            var action = new Func<Task<IActionResult>>(async () =>
            {
                var apiResult = new ApiResult<string>();
                if (categoryProduct == null)
                {
                    apiResult.StatusCode = ApiStatusCode.BadRequest;
                    apiResult.Message = "Invalid category product request data";
                }
                else
                {
                    var categoryResult = await _context.CategoryProducts.FindAsync(categoryProduct.Id);
                    if (categoryResult == null)
                    {
                        apiResult.StatusCode = ApiStatusCode.NotFound;
                        apiResult.Message = "Category of product not found";
                    }
                    else
                    {
                        categoryResult.Name = categoryProduct.Name;

                        _context.CategoryProducts.Update(categoryResult);
                        await _context.SaveChangesAsync();

                        apiResult.StatusCode = ApiStatusCode.OK;
                        apiResult.Message = "Category product updated successfully";
                    }
                }

                return _commonMethod.HandleApiResultAsync(apiResult);
            });

            return await ErrorHandler.ExecuteAsync(action);
        }

        public async Task<IActionResult> DeleteAsync(string? id)
        {
            var action = new Func<Task<IActionResult>>(async () =>
            {
                var apiResult = new ApiResult<string>();
                var categoryProduct = await _context.CategoryProducts.FindAsync(id);
                if (categoryProduct == null)
                {
                    apiResult.StatusCode = ApiStatusCode.NotFound;
                    apiResult.Message = "Category Product not found";
                }
                else
                {
                    _context.CategoryProducts.Remove(categoryProduct);
                    await _context.SaveChangesAsync();

                    apiResult.StatusCode = ApiStatusCode.OK;
                    apiResult.Message = "Category Product deleted successfully";
                }

                return _commonMethod.HandleApiResultAsync(apiResult);
            });

            return await ErrorHandler.ExecuteAsync(action);
        }
    }
}