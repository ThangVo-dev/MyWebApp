using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Service.Common
{
    public class ErrorHandler
    {
        public async static Task<IActionResult> ExecuteAsync(Func<Task<IActionResult>> action)
        {
            try
            {
                return await action();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return new ConflictObjectResult(new
                {
                    Success = false,
                    Message = "A concurrency error occurred.",
                    Details = ex.Message
                });
            }
            catch (DbUpdateException ex)
            {
                return new ObjectResult(new
                {
                    Success = false,
                    Message = "A database error occurred.",
                    Details = ex.Message
                })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
            catch (Exception ex)
            {
                return new ObjectResult(new
                {
                    Success = false,
                    Message = "An unexpected error occurred.",
                    Details = ex.Message
                })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }
    }
}