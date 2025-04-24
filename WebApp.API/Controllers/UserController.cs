using AppWeb.Service.API;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Shared.Models.Requests;
using WebApp.Shared.Models.User;

[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private AuthService _authService;
    private UserService _userService;

    public UserController(AuthService authService, UserService userService)
    {
        _authService = authService;
        _userService = userService;
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        var result = await _authService.LoginAsync(loginRequest);
        return result;
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpGet]
    [Route("get/{id}")]
    public async Task<IActionResult> GetUserById(string? id)
    {
        var result = await _userService.GetUserByIdAsync(id);
        return result;
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPut]
    [Route("edit-profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UserMdl request)
    {
        var result = await _userService.UpdateProfileAsync(request);
        return result;
    }
}