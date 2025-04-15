using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WebApp.Data.Entities;
using WebApp.Service.Common;
using WebApp.Service.Enums;
using WebApp.Service.ResultModels;
using WebApp.Shared.Models.Requests;
using WebApp.Shared.Models.Responses;

namespace AppWeb.Service.API
{
    public class UserService
    {
        private readonly WebAppContext _context;
        private readonly CommonMethod _commonMethod;

        public UserService(WebAppContext context, CommonMethod commonMethod)
        {
            _context = context;
            _commonMethod = commonMethod;
        }

        public async Task<IActionResult> GetUserByIdAsync(string? id)
        {
            var action = new Func<Task<IActionResult>>(async () =>
            {
                var authResult = new ApiResult<UserResponse>();
                if (!string.IsNullOrEmpty(id))
                {
                    var user = await _context.Users.Include(x => x.Role).FirstOrDefaultAsync(u => u.Id == id);
                    if (user == null)
                    {
                        authResult.StatusCode = ApiStatusCode.NotFound;
                        authResult.Message = "User not found";
                    }
                    else
                    {
                        var userResponse = new UserResponse
                        {
                            Id = user?.Id,
                            UserName = user?.UserName,
                            FullName = user?.FullName,
                            PhoneNumber = user?.PhoneNumber,
                            Email = user?.Email,
                            RoleName = user?.Role?.Name
                        };

                        authResult.StatusCode = ApiStatusCode.OK;
                        authResult.Data = userResponse;
                    }
                }
                else
                {
                    authResult.StatusCode = ApiStatusCode.BadRequest;
                    authResult.Message = "Invalid user ID";
                }

                var commonMethod = new CommonMethod();
                var result = commonMethod.HandleApiResultAsync<UserResponse>(authResult);
                return result;
            });

            return await ErrorHandler.ExecuteAsync(action);
        }

        public async Task<IActionResult> UpdateProfileAsync(string? id, EditProfileRequest request)
        {
            var action = new Func<Task<IActionResult>>(async () =>
            {
                var apiResult = new ApiResult<UserResponse>();
                if (string.IsNullOrEmpty(id) || request == null || id != request.Id)
                {
                    apiResult.StatusCode = ApiStatusCode.BadRequest;
                    apiResult.Message = "Invalid user ID or request data";
                }
                else if (string.IsNullOrWhiteSpace(request?.FullName))
                {
                    apiResult.StatusCode = ApiStatusCode.BadRequest;
                    apiResult.Message = "FullName not empty.";
                }

                else if (string.IsNullOrWhiteSpace(request?.PhoneNumber))
                {
                    apiResult.StatusCode = ApiStatusCode.BadRequest;
                    apiResult.Message = "PhoneNumber not empty.";
                }

                // if (!Regex.IsMatch(request?.PhoneNumber, @"^\+?[0-9]{10,15}$"))
                // {
                //     return new BadRequestObjectResult(new
                //     {
                //         Success = false,
                //         Message = "Invalid phone number format."
                //     });
                // }
                else
                {
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
                    if (user == null)
                    {
                        apiResult.StatusCode = ApiStatusCode.NotFound;
                        apiResult.Message = "User not found";
                    }
                    else
                    {
                        user.FullName = request?.FullName;
                        user.PhoneNumber = request?.PhoneNumber;

                        _context.Users.Update(user);
                        await _context.SaveChangesAsync();

                        apiResult.StatusCode = ApiStatusCode.OK;
                        apiResult.Message = "Profile updated successfully";
                        apiResult.Data = new UserResponse
                        {
                            Id = user?.Id,
                            UserName = user?.UserName,
                            FullName = user?.FullName,
                            PhoneNumber = user?.PhoneNumber,
                            Email = user?.Email
                        };
                    }
                }

                return _commonMethod.HandleApiResultAsync(apiResult);
            });

            return await ErrorHandler.ExecuteAsync(action);
        }
    }
}