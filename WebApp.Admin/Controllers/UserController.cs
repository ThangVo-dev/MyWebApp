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
using WebApp.Shared.Responses;

namespace WebApp.MVC.Controllers
{
    public class UserController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public UserController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
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
            if (!ModelState.IsValid)
            {
                return View("Login", loginRequest);
            }

            var client = _httpClientFactory.CreateClient("ProductApi");
            var content = new StringContent(JsonConvert.SerializeObject(loginRequest), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/user/login", content);

            if (response.IsSuccessStatusCode)
            {
                var jsonResult = await response.Content.ReadAsStringAsync();
                var data = JObject.Parse(jsonResult)["data"]?.ToString();
                if (string.IsNullOrEmpty(data))
                {
                    TempData["ErrorMessage"] = "Failed to retrieve token data.";
                    return View("Login", loginRequest);
                }

                var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(data);

                if (!string.IsNullOrEmpty(tokenResponse?.Token))
                {
                    // Save token to session
                    HttpContext.Session.SetString("JwtToken", tokenResponse.Token);

                    // Decode the token to extract FullName
                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(tokenResponse.Token);
                    var fullName = jwtToken.Claims.FirstOrDefault(c => c.Type == "FullName")?.Value;

                    // Save FullName to Session
                    HttpContext.Session.SetString("FullName", fullName ?? "Guest");

                    TempData["SuccessMessage"] = "Login success";
                    return RedirectToAction("Index", "Product");
                }

                TempData["ErrorMessage"] = "Failed to retrieve token.";
                return View("Login", loginRequest);
            }

            TempData["ErrorMessage"] = "Invalid username or password.";
            return View("Login", loginRequest);
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
                    ExpirationMinutes = 60
                };

                var commonMethod = new CommonMethod();
                var jwtToken = commonMethod.GenerateJwtToken(claimJWTToken);
                HttpContext.Session.SetString("JwtToken", jwtToken);
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
            // Delete the authentication cookie
            await HttpContext.SignOutAsync();

            // Delete the JWT token from session (if any)
            HttpContext.Session.Remove("JwtToken");

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

            var client = _httpClientFactory.CreateClient("ProductApi");
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
        public IActionResult EditProfile()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            // Decode the token to extract FullName
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            // var username = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
            var fullName = jwtToken.Claims.FirstOrDefault(c => c.Type == "FullName")?.Value;
            var phoneNumber = jwtToken.Claims.FirstOrDefault(c => c.Type == "PhoneNumber")?.Value;
            var profile = new EditProfileRequest
            {
                FullName = fullName,
                PhoneNumber = phoneNumber
            };

            return View(profile);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(EditProfileRequest editProfileRequest)
        {
            if (!ModelState.IsValid)
            {
                return View(editProfileRequest);
            }

            var client = _httpClientFactory.CreateClient("ProductApi");
            var content = new StringContent(JsonConvert.SerializeObject(editProfileRequest), Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"api/user/edit-profile", content);
            // var response = await client.PutAsync($"api/user/edit-profile/{editProfileRequest.Id}", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            try
            {
                if (response.IsSuccessStatusCode)
                {
                    var jsonObject = JObject.Parse(responseContent);
                    var message = JObject.Parse(responseContent)["message"]?.ToString();
                    TempData["SuccessMessage"] = message;
                    return RedirectToAction("Index", "Product");
                }
                else
                {
                    TempData["ErrorMessage"] = responseContent.ToString();
                    return View(editProfileRequest);
                }
            }
            catch (JsonReaderException)
            {
                TempData["ErrorMessage"] = responseContent;
                return View(editProfileRequest);
            }
        }
    }
}