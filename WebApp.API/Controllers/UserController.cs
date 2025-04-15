using AppWeb.Service.API;
using Microsoft.AspNetCore.Mvc;
using WebApp.Shared.Models.Requests;

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

    [HttpGet]
    [Route("get/{id}")]
    public async Task<IActionResult> GetUserById(string? id)
    {
        var result = await _userService.GetUserByIdAsync(id);
        return result;
    }

    [HttpPut]
    [Route("edit-profile/{id}")]
    public async Task<IActionResult> UpdateProfile(string? id, [FromBody] EditProfileRequest request)
    {
        var result = await _userService.UpdateProfileAsync(id, request);
        return result;
    }
}