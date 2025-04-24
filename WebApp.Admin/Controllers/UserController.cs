using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using WebApp.Service.Common;
using WebApp.Shared.Models.Common;
using WebApp.Shared.Models.Requests;
using WebApp.Shared.Models.Responses;
using WebApp.Shared.Models.User;
using WebApp.Shared.Responses;
using WebApp.Admin.Common;
using WebApp.Shared.Helpers;

namespace WebApp.MVC.Controllers
{
    public class UserController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly MethodCommon _methodCommon;

        public UserController(IHttpClientFactory httpClientFactory, IConfiguration configuration, MethodCommon methodCommon)
        {
            _methodCommon = methodCommon;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Login()
        {
            ViewData["IsLoginPage"] = true;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest loginRequest)
        {
            ViewData["IsLoginPage"] = true;
            if (!ModelState.IsValid)
            {
                return View("Login", loginRequest);
            }
            else
            {
                var client = _httpClientFactory.CreateClient("WebAppApi");
                var content = new StringContent(JsonConvert.SerializeObject(loginRequest), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("api/user/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResult = await response.Content.ReadAsStringAsync();
                    var data = JObject.Parse(jsonResult)["data"]?.ToString();

                    var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(data ?? string.Empty);

                    if (!string.IsNullOrEmpty(tokenResponse?.Token))
                    {
                        // Save token to session
                        // HttpContext.Session.SetString("JwtToken", tokenResponse.Token);

                        var cookieOptions = new CookieOptions
                        {
                            HttpOnly = true,
                            Expires = DateTimeOffset.UtcNow.AddMinutes(30) // or adjust as needed
                        };
                        Response.Cookies.Append("JwtToken", tokenResponse.Token, cookieOptions);

                        // Decode the token to extract FullName
                        var handler = new JwtSecurityTokenHandler();
                        var jwtToken = handler.ReadJwtToken(tokenResponse.Token);

                        // Save FullName to Cookie instead of Session
                        var fullName = jwtToken.Claims.FirstOrDefault(c => c.Type == "FullName")?.Value;
                        var fullNameCookieOptions = new CookieOptions
                        {
                            HttpOnly = false,
                            Expires = DateTimeOffset.UtcNow.AddMinutes(30)
                        };
                        Response.Cookies.Append("FullName", fullName ?? "Guest", fullNameCookieOptions);

                        ViewData["IsLoginPage"] = false;
                        TempData["SuccessMessage"] = $"Login success - Token is valid. Expire in {(jwtToken.ValidTo - DateTime.UtcNow).TotalMinutes:F2} minutes.";
                        return RedirectToAction("Index", "Product");
                    }

                    TempData["ErrorMessage"] = "Failed to retrieve token.";
                    return View("Login", loginRequest);
                }
                else
                {
                    TempData["ErrorMessage"] = "Api login failed.";
                    return View("Login", loginRequest);
                }
            }
        }

        [HttpGet]
        public IActionResult LoginGoogle()
        {
            //Redirect user to Google for authentication
            var redirectUrl = Url.Action("GoogleCallback", "User");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public async Task<IActionResult> GoogleCallback()
        {
            //Get user information from Google after authentication
            var authenticateResult = await HttpContext.AuthenticateAsync();

            if (!authenticateResult.Succeeded)
            {
                TempData["ErrorMessage"] = "Google login failed.";
                return RedirectToAction("Login");
            }

            // Get user information from claims
            var claims = authenticateResult.Principal.Identities.FirstOrDefault()?.Claims;
            var email = claims?.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
            var name = claims?.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Name)?.Value;

            var googleAccessToken = await HttpContext.GetTokenAsync("access_token");
            if (!string.IsNullOrEmpty(googleAccessToken) && await ValidateGoogleAccessToken(googleAccessToken))
            {
                var issuer = _configuration["JwtSettings:Issuer"];
                var audience = _configuration["JwtSettings:Audience"];
                var claimJWTToken = new ClaimJWTToken
                {
                    Username = email,
                    Role = "User",
                    Issuer = _configuration["JwtSettings:Issuer"],
                    Audience = _configuration["JwtSettings:Audience"],
                    SigningKey = _configuration["JwtSettings:IssuerSigningKey"],
                    ExpirationMinutes = 30
                };

                var commonMethod = new CommonMethod();
                var jwtToken = commonMethod.GenerateJwtToken(claimJWTToken);
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(30) // or adjust as needed
                };
                Response.Cookies.Append("JwtToken", jwtToken, cookieOptions);

                // Save FullName to Cookie instead of Session
                var fullNameCookieOptions = new CookieOptions
                {
                    HttpOnly = false,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(30)
                };
                Response.Cookies.Append("FullName", name ?? "Guest", fullNameCookieOptions);
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to retrieve Google access token.";
                return RedirectToAction("Login");
            }

            TempData["SuccessMessage"] = $"Welcome, {name} ({email}!";
            return RedirectToAction("Index", "Product");
        }


        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // Sign out any authentication
            await HttpContext.SignOutAsync();

            // Remove cookies
            Response.Cookies.Delete("JwtToken");
            Response.Cookies.Delete("FullName");

            // Clear session just in case
            HttpContext.Session.Clear();

            // Redirect to the Login page
            return RedirectToAction("Login", "User");
        }

        public async Task<bool> ValidateGoogleAccessToken(string accessToken)
        {
            using var client = new HttpClient();
            var response = await client.GetAsync($"https://www.googleapis.com/oauth2/v1/tokeninfo?access_token={accessToken}");
            return response.IsSuccessStatusCode;
        }

        [HttpGet]
        public IActionResult LoginFacebook()
        {
            var redirectUrl = Url.Action("FacebookCallback", "User");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, "Facebook");
        }

        [HttpGet]
        public async Task<IActionResult> FacebookCallback()
        {
            var authenticateResult = await HttpContext.AuthenticateAsync();

            if (!authenticateResult.Succeeded)
            {
                TempData["ErrorMessage"] = "Facebook login failed.";
                return RedirectToAction("Login");
            }

            // Get user information from claims
            var claims = authenticateResult.Principal.Identities.FirstOrDefault()?.Claims;
            var email = claims?.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
            var name = claims?.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Name)?.Value;

            // Generate JWT token (if needed)
            var issuer = _configuration["JwtSettings:Issuer"];
            var audience = _configuration["JwtSettings:Audience"];
            var claimJWTToken = new ClaimJWTToken
            {
                Username = email,
                Role = "User",
                Issuer = issuer,
                Audience = audience,
                SigningKey = _configuration["JwtSettings:IssuerSigningKey"],
                ExpirationMinutes = 60
            };

            var commonMethod = new CommonMethod();
            var jwtToken = commonMethod.GenerateJwtToken(claimJWTToken);
            HttpContext.Session.SetString("JwtToken", jwtToken);

            TempData["SuccessMessage"] = $"Welcome, {name} ({email})!";
            return RedirectToAction("Index", "Product");
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserRequest createUserRequest)
        {
            if (!ModelState.IsValid)
            {
                return View(createUserRequest);
            }

            var client = _httpClientFactory.CreateClient("WebAppApi");
            var content = new StringContent(JsonConvert.SerializeObject(createUserRequest), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/user/create", content);
            var responseContent = await response.Content.ReadAsStringAsync();
            var message = JObject.Parse(responseContent)["message"]?.ToString();

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = message;
                return RedirectToAction("Index", "User");
            }

            TempData["ErrorMessage"] = message;
            return View(createUserRequest);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var client = _httpClientFactory.CreateClient("WebAppApi");
            var token = Request.Cookies["JwtToken"];
            var validToken = TokenValidator.IsTokenValid(token, out var msg);
            if (validToken)
            {
                // Add token to the header if valid
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
                var response = await client.GetAsync($"api/user/get/{userId}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResult = await response.Content.ReadAsStringAsync();
                    var data = JObject.Parse(jsonResult)["data"]?.ToString();
                    var user = JsonConvert.DeserializeObject<UserMdl>(data ?? string.Empty);
                    if (user == null)
                    {
                        TempData["ErrorMessage"] = "User is null.";
                        return RedirectToAction(nameof(ProductController.Index), ControllerNames.Product);
                    }
                    else
                    {
                        ViewData["IsProfilePage"] = true;
                        return View(user);
                    }

                }
                TempData["ErrorMessage"] = "Api get user is invalid.";
                return RedirectToAction(nameof(Index), "Product");
            }

            TempData["ErrorMessage"] = msg;
            return RedirectToAction("Login", "User");
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(UserMdl userMdl, IFormFile? avatarFile)
        {
            ModelState.Remove("Id");
            if (!ModelState.IsValid)
            {
                return View(userMdl);
            }

            // Handle avatar upload if file is present
            if (avatarFile != null && avatarFile.Length > 0)
            {
                var hash = _methodCommon.ComputeFileHash(avatarFile);
                var fileName = $"{hash}{Path.GetExtension(avatarFile.FileName)}";
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/avatars");
                Directory.CreateDirectory(uploadPath);
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await avatarFile.CopyToAsync(stream);
                }

                userMdl.AvatarUrl = $"/uploads/avatars/{fileName}";
            }

            var client = _httpClientFactory.CreateClient("WebAppApi");
            var token = Request.Cookies["JwtToken"];
            var validToken = TokenValidator.IsTokenValid(token, out var msg);
            if (validToken)
            {
                // Add token to the header if valid
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
                userMdl.Id = userId;

                var content = new StringContent(JsonConvert.SerializeObject(userMdl), Encoding.UTF8, "application/json");
                var response = await client.PutAsync($"api/user/edit-profile", content);
                var jsonResult = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var data = JObject.Parse(jsonResult)["data"]?.ToString();
                    var user = JsonConvert.DeserializeObject<UserResponse>(data ?? string.Empty);

                    var fullNameCookieOptions = new CookieOptions
                    {
                        HttpOnly = false,
                        Expires = DateTimeOffset.UtcNow.AddMinutes(30)
                    };
                    Response.Cookies.Append("FullName", user?.FullName ?? "Guest", fullNameCookieOptions);

                    var AvatarUrlCookieOptions = new CookieOptions
                    {
                        HttpOnly = false,
                        Expires = DateTimeOffset.UtcNow.AddMinutes(30)
                    };
                    Response.Cookies.Append("AvatarUrl", user?.AvatarUrl ?? "~/images/default_avatar.png", AvatarUrlCookieOptions);

                    var message = JObject.Parse(jsonResult)["message"]?.ToString();
                    TempData["SuccessMessage"] = message;
                    return RedirectToAction(nameof(Profile));
                }
                else
                {
                    TempData["ErrorMessage"] = "Api error occurred.";
                    return RedirectToAction("Profile");
                }
            }

            TempData["ErrorMessage"] = msg;
            return RedirectToAction("Login", "User");
        }

        [HttpPost]
        public async Task<IActionResult> UploadAvatar(IFormFile avatar)
        {
            if (avatar == null || avatar.Length == 0)
            {
                TempData["ErrorMessage"] = "No file selected.";
                return RedirectToAction("Profile");
            }

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(avatar.FileName)}";
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/avatars");
            Directory.CreateDirectory(uploadPath); // Sure the directory exists

            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await avatar.CopyToAsync(stream);
            }

            var relativePath = $"/uploads/avatars/{fileName}";

            // Lưu path này vào DB cho user (ví dụ gọi API edit-profile)

            TempData["SuccessMessage"] = "Avatar updated.";
            return RedirectToAction("Profile");
        }
    }
}